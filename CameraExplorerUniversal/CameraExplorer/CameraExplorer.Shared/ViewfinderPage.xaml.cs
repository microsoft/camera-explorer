
using CameraExplorer.Models;
using CameraExplorer.Utilities;
using CameraExplorer.ViewModels;
using Nokia.Graphics.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CameraExplorer
{

    public sealed partial class ViewfinderPage : Page
    {

        private SimpleOrientationSensor _simpleOrientationSensor;
        private DispatcherTimer _focusReticleBlinkTimer;
        private IAsyncAction _focusAction;
        private bool _pinchZooming;
        private RegionOfInterest _regionOfInterest;
        private CameraSession _cameraSession;
        private CameraSettings _cameraSettings;
        private WriteableBitmap _previewBitmap;
        private StorageFile _savedPhoto;

        public ViewfinderPage()
        {
            this.InitializeComponent();

            _focusReticleBlinkTimer = new DispatcherTimer();
            _focusReticleBlinkTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _focusReticleBlinkTimer.Tick += (s, e) => { FocusReticle.Visibility = (FocusReticle.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible; };

            _simpleOrientationSensor = SimpleOrientationSensor.GetDefault();
            _simpleOrientationSensor.OrientationChanged += SimpleOrientationSensor_OrientationChanged;

            _cameraSession = CameraSession.Instance;
            _cameraSettings = CameraSession.Instance.Settings;
            DataContext = new CameraSettingsViewModel(CameraSession.Instance.MediaCapture, CameraSession.Instance.Settings);
            _cameraSession.StateChanged += CameraSession_StateChanged;

            Windows.Phone.UI.Input.HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            Windows.Phone.UI.Input.HardwareButtons.CameraHalfPressed += HardwareButtons_CameraHalfPressed;
            Windows.Phone.UI.Input.HardwareButtons.CameraReleased += HardwareButtons_CameraReleased;

            ViewFinderCanvas.SizeChanged += ViewFinderCanvas_SizeChanged;
        }

        private async void CameraSession_StateChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   CameraSessionState state = ((CameraSession.StateChangedEventArgs)e).State;
                   switch (state)
                   {
                       case CameraSessionState.Init:
                           SettingsGrid.Visibility = Visibility.Collapsed;
                           break;
                       case CameraSessionState.Preview:
                           SettingsGrid.Visibility = Visibility.Visible;
                           FocusReticle.Visibility = Visibility.Collapsed;
                           break;
                       case CameraSessionState.Capture:
                           SettingsGrid.Visibility = Visibility.Collapsed;
                           break;
                       case CameraSessionState.Closed:
                           SettingsGrid.Visibility = Visibility.Collapsed;
                           break;
                   }
               });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _previewBitmap = new WriteableBitmap(640, 480);

            _cameraSession.SetViewfinder(ViewFinder);

            if (_cameraSession.MediaCapture.VideoDeviceController.FocusControl.FocusChangedSupported)
            {
                _cameraSession.MediaCapture.FocusChanged += MediaCapture_FocusChanged;
            }

            DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            _simpleOrientationSensor.GetCurrentOrientation();

            await AdjustToOrientation(_simpleOrientationSensor.GetCurrentOrientation());
        }

        private async void MediaCapture_FocusChanged(MediaCapture sender, MediaCaptureFocusChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   if (args.FocusState == MediaCaptureFocusState.Searching)
                   {
                       if (!_focusReticleBlinkTimer.IsEnabled)
                       {
                           _focusReticleBlinkTimer.Start();
                           FocusReticle.Visibility = Visibility.Visible;
                       }
                   }
                   else
                   {
                       if (_focusReticleBlinkTimer.IsEnabled)
                       {
                           _focusReticleBlinkTimer.Stop();
                           FocusReticle.Visibility = Visibility.Visible;
                       }
                   }

                   switch (args.FocusState)
                   {
                       case MediaCaptureFocusState.Searching:
                           FocusReticle.BorderBrush = new SolidColorBrush(Colors.White);
                           break;
                       case MediaCaptureFocusState.Focused:
                           FocusReticle.BorderBrush = new SolidColorBrush(Colors.White);
                           break;
                       case MediaCaptureFocusState.Failed:
                           FocusReticle.BorderBrush = new SolidColorBrush(Colors.Red);
                           break;
                       case MediaCaptureFocusState.Lost:
                           FocusReticle.BorderBrush = new SolidColorBrush(Colors.Red);
                           break;
                   }
               });
        }

        private async void HardwareButtons_CameraHalfPressed(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Collapsed;

            Canvas.SetLeft(FocusReticle, ViewFinderCanvas.ActualWidth / 2 - FocusReticle.ActualWidth / 2);
            Canvas.SetTop(FocusReticle, ViewFinderCanvas.ActualHeight / 2 - FocusReticle.ActualHeight / 2);

            await CameraSession.Instance.MediaCapture.VideoDeviceController.RegionsOfInterestControl.ClearRegionsAsync();
            _focusAction = CameraSession.Instance.MediaCapture.VideoDeviceController.FocusControl.FocusAsync();
        }

        private async void HardwareButtons_CameraPressed(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {
            await Capture();
        }

        private void HardwareButtons_CameraReleased(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {

            if (_focusReticleBlinkTimer.IsEnabled)
                _focusReticleBlinkTimer.Stop();

            SettingsGrid.Visibility = Visibility.Visible;
            FocusReticle.Visibility = Visibility.Collapsed;

            if (_focusAction != null)
            {
                _focusAction.Cancel();
                _focusAction = null;
            }
        }

        private void ViewFinderCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            ViewFinder.Width = ViewFinderCanvas.ActualWidth;
            ViewFinder.Height = ViewFinderCanvas.ActualHeight;

            SettingsGrid.Width = ViewFinderCanvas.ActualWidth;
            SettingsGrid.Height = ViewFinderCanvas.ActualHeight;

            AdjustToOrientation(_simpleOrientationSensor.GetCurrentOrientation());
        }

        private async void SimpleOrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.Orientation);
            await AdjustToOrientation(args.Orientation);
        }

        private async Task AdjustToOrientation(SimpleOrientation orientation)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   switch (orientation)
                   {
                       case SimpleOrientation.Rotated90DegreesCounterclockwise:
                           {
                               RotateItemsInSettingsGrid(0);
                           }
                           break;

                       case SimpleOrientation.Rotated270DegreesCounterclockwise:
                           {
                               RotateItemsInSettingsGrid(180);
                           }
                           break;

                       case SimpleOrientation.Rotated180DegreesCounterclockwise:
                           {
                               RotateItemsInSettingsGrid(90);
                           }
                           break;

                       default:
                           {
                               RotateItemsInSettingsGrid(-90);
                           }
                           break;
                   }
                   SettingsGrid.UpdateLayout();
                   InvalidateArrange();
                   InvalidateMeasure();
               });
        }

        private void RotateItemsInSettingsGrid(double angle)
        { 

            int itemCount = VisualTreeHelper.GetChildrenCount(SettingsGrid);
            for (int i = 0; i < itemCount; i++)
            {
                var item = VisualTreeHelper.GetChild(SettingsGrid, i) as FrameworkElement;
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.CenterX = item.ActualWidth / 2;
                rotateTransform.CenterY = item.ActualHeight / 2;
                rotateTransform.Angle = angle;
                item.RenderTransform = rotateTransform;
            }
        }

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (CameraSession.Instance.State != CameraSessionState.Preview)
                return;

            SettingsGrid.Visibility = Visibility.Collapsed;
            if (_regionOfInterest != null || !CameraSession.Instance.MediaCapture.VideoDeviceController.FocusControl.Supported)
            {
                await Capture();
            }
            else
            {
                Canvas.SetLeft(FocusReticle, ViewFinderCanvas.ActualWidth / 2 - FocusReticle.Width / 2);
                Canvas.SetTop(FocusReticle, ViewFinderCanvas.ActualHeight / 2 - FocusReticle.Height / 2);
                await CameraSession.Instance.MediaCapture.VideoDeviceController.FocusControl.FocusAsync().AsTask().ContinueWith(t => Capture());
            }
        }

        private async Task Capture()
        {
            if (CameraSession.Instance.State != CameraSessionState.Preview)
                return;

            _savedPhoto = await KnownFolders.CameraRoll.CreateFileAsync("CameraExplorer.jpg", CreationCollisionOption.GenerateUniqueName);

            using (InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream())
            {
                await CameraSession.Instance.Capture(memoryStream, CameraUtilities.GetExifOrientationForDeviceOrientation(_simpleOrientationSensor.GetCurrentOrientation()));

                memoryStream.AsStream().Position = 0;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                   async () =>
                   {
                       
                       memoryStream.AsStream().Position = 0;
                       RandomAccessStreamImageSource i = new RandomAccessStreamImageSource(memoryStream);
                       using (WriteableBitmapRenderer r = new WriteableBitmapRenderer())
                       {
                           r.WriteableBitmap = _previewBitmap;
                           r.Source = i;
                           await r.RenderAsync();
                       }
                       CapturedImage.Source = _previewBitmap;
                       _previewBitmap.Invalidate();
                   });

                memoryStream.AsStream().Position = 0;
                memoryStream.AsStream().CopyTo(await _savedPhoto.OpenStreamForWriteAsync());
            }

            _regionOfInterest = null;
        }

        public async void SetVideoPreviewResolution()
        {
            var properties = CameraSession.Instance.MediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);

            System.Collections.Generic.IReadOnlyList<IMediaEncodingProperties> res;
            res = CameraSession.Instance.MediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
            uint maxResolution = 0;
            int indexMaxResolution = 0;

            if (res.Count >= 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    VideoEncodingProperties vp = (VideoEncodingProperties)res[i];

                    if (vp.Width > maxResolution)
                    {
                        indexMaxResolution = i;
                        maxResolution = vp.Width;
                        Debug.WriteLine("Preview resolution: " + vp.Width);
                    }
                }
                await CameraSession.Instance.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, res[indexMaxResolution]);
            }
        }

        private async Task Focus(double x, double y)
        {
            
            var videoPreviewProperties = CameraSession.Instance.MediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            Canvas.SetLeft(FocusReticle, x - FocusReticle.Width / 2);
            Canvas.SetTop(FocusReticle, y - FocusReticle.Height / 2);

            _regionOfInterest = new RegionOfInterest();

            // Map coordinates from CaptureElement (UI) coordinates to actual video preview image coordinates
            double roiX = (x / ViewFinder.ActualWidth) * videoPreviewProperties.Width;
            double roiY = (y / ViewFinder.ActualHeight) * videoPreviewProperties.Height;
            double roiW = videoPreviewProperties.Width / 10;

            var videoDeviceController = CameraSession.Instance.MediaCapture.VideoDeviceController;
            
            Rect bounds = new Rect(x, y, roiW, roiW);
            _regionOfInterest.AutoWhiteBalanceEnabled = videoDeviceController.RegionsOfInterestControl.AutoWhiteBalanceSupported;
            _regionOfInterest.AutoExposureEnabled = videoDeviceController.RegionsOfInterestControl.AutoExposureSupported;
            _regionOfInterest.AutoFocusEnabled = videoDeviceController.RegionsOfInterestControl.AutoFocusSupported;
            _regionOfInterest.Bounds = bounds;

            videoDeviceController.WhiteBalance.TrySetAuto(videoDeviceController.RegionsOfInterestControl.AutoWhiteBalanceSupported);

            await videoDeviceController.RegionsOfInterestControl.SetRegionsAsync(new RegionOfInterest[] { _regionOfInterest });

            if (videoDeviceController.FocusControl.Supported)
                await videoDeviceController.FocusControl.FocusAsync();
        }

        private void ViewFinder_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pinchZooming = false;
        }

        private async void ViewFinder_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_pinchZooming)
            {
                await Focus(
                    (e.GetCurrentPoint(ViewFinder).Position.X),
                    (e.GetCurrentPoint(ViewFinder).Position.Y)
                    );
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CameraSettingsPage));
        }

        private void ViewFinder_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (CameraSession.Instance.MediaCapture.VideoDeviceController.ZoomControl.Supported)
            {          
                _pinchZooming = true;
                double tmp = CameraSession.Instance.Settings.Zoom * e.Delta.Scale;
                var zoomCapabilities = CameraSession.Instance.MediaCapture.VideoDeviceController.Zoom.Capabilities;
                if (tmp > zoomCapabilities.Max)
                    tmp = zoomCapabilities.Max;
                else if (tmp < 1)
                    tmp = 1;
                CameraSession.Instance.Settings.Zoom = tmp;
            }
        }

        private void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            CameraSession.Instance.Settings.NextFlashMode();
        }

        private async void CameraDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            await CameraSession.Instance.NextDevice();
            DataContext = new CameraSettingsViewModel(CameraSession.Instance.MediaCapture, CameraSession.Instance.Settings);

            if (_cameraSession.MediaCapture.VideoDeviceController.FocusControl.FocusChangedSupported)
            {
                _cameraSession.MediaCapture.FocusChanged += MediaCapture_FocusChanged;
            }
        }

        private async void CapturedImage_Tapped(object sender, TappedRoutedEventArgs e)        
        {
            if (_savedPhoto != null)
            {
                System.Diagnostics.Debug.WriteLine("Opening " + _savedPhoto.Path);
                await CameraSession.Instance.Close();
                await Windows.System.Launcher.LaunchFileAsync(_savedPhoto);
            }
        }

    }
}
