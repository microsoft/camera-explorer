/*
 * Copyright (c) 2012-2014 Microsoft Mobile. All rights reserved.
 * See the license file delivered with this project for more information.
 */

using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    /// <summary>
    /// Application main page containing the viewfinder with overlays.
    /// Two methods for capturing a photo are available: pressing a capture
    /// icon on the screen and pressing the hardware shutter release key.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Constants
        private readonly Windows.Foundation.Size DefaultCameraResolution =
            new Windows.Foundation.Size(640, 480);

        // Members
        private CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;
        private ProgressIndicator _progressIndicator = new ProgressIndicator();
        private bool _capturing = false;
        private Semaphore _focusSemaphore = new Semaphore(1, 1);
        private bool _manuallyFocused = false;
        private Windows.Foundation.Size _focusRegionSize = new Windows.Foundation.Size(80, 80);
        private SolidColorBrush _notFocusedBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush _focusedBrush = new SolidColorBrush(Colors.Green);

        private ApplicationBarIconButton _sensorButton = null;
        private ApplicationBarIconButton _captureButton = null;
        private ApplicationBarIconButton _settingsButton = null;
        
        private CameraSensorLocation _sensorLocation = CameraSensorLocation.Back;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            VideoCanvas.Tap += new EventHandler<GestureEventArgs>(videoCanvas_Tap);

            DataContext = _dataContext;

            _progressIndicator.IsIndeterminate = true;

            CreateAppBar();
            ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem {Text = "about", IsEnabled = false};
            ApplicationBar.MenuItems.Add(menuItem);
            menuItem.Click += new EventHandler(aboutMenuItem_Click);
        }

        /// <summary>
        /// If camera has not been initialized when navigating to this page, initialization
        /// will be started asynchronously in this method. Once initialization has been
        /// completed the camera will be set as a source to the VideoBrush element
        /// declared in XAML. On-screen controls are enabled when camera has been initialized.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("MainPage.OnNavigatedTo()");

            if (_dataContext.Device == null)
            {
                ShowProgress("Initializing camera...");
                await InitializeCamera(_sensorLocation);
                HideProgress();
            }

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device != null && _dataContext.Device.SensorLocation == CameraSensorLocation.Back ?
                           _dataContext.Device.SensorRotationInDegrees :
                         - _dataContext.Device.SensorRotationInDegrees
            };

            videoBrush.SetSource(_dataContext.Device);

            overlayComboBox.Opacity = 1;

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// On-screen controls are disabled when navigating away from the viewfinder. This is because
        /// we want the controls to default to disabled when arriving to the page again.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            /* Release camera as soon as no longer needed in order to avoid green bitmap bug
             * If the page to navigate to is the settings page, we keep the camera alive.
             */
            if (_dataContext.Device != null && !e.Uri.ToString().Contains("SettingsPage.xaml"))
            {
                Debug.WriteLine("MainPage.OnNavigatingFrom(): Releasing camera.");
                _dataContext.Device.Dispose();
                _dataContext.Device = null;
            }

            overlayComboBox.Opacity = 0;

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Enables or disabled on-screen controls.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable controls.</param>
        private void SetScreenButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
            {
                b.IsEnabled = enabled;
            }

            foreach (ApplicationBarMenuItem m in ApplicationBar.MenuItems)
            {
                m.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// Enables or disables listening to hardware shutter release key events.
        /// </summary>
        /// <param name="enabled">True to enable listening, false to disable listening.</param>
        private void SetCameraButtonsEnabled(bool enabled)
        {
            if (enabled)
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed += ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed += ShutterKeyPressed;
            }
            else
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed -= ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed -= ShutterKeyPressed;
            }
        }

        /// <summary>
        /// Clicking on the settings button begins navigating to the settings page.
        /// </summary>
        private void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Clicking on sensor button disables camera capturing controls, uninitializes old
        /// camera instance and initializes new camera instance using the other sensor. On-screen
        /// controls and listening to hardware shutter release key is disabled while initializing the
        /// sensor because capturing a photo is not possible at that time.
        /// </summary>
        private async void sensorButton_Click(object sender, EventArgs e)
        {
            FocusIndicator.SetValue(Canvas.VisibilityProperty, Visibility.Collapsed);
            _manuallyFocused = false;

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            ShowProgress("Initializing camera...");

            videoBrush.Opacity = 0.25;

            overlayComboBox.Opacity = 0;

            _dataContext.Device.Dispose();
            _dataContext.Device = null;

            IReadOnlyList<CameraSensorLocation> sensorLocations = PhotoCaptureDevice.AvailableSensorLocations;

            if (_sensorLocation == sensorLocations[1])
            {
                _sensorLocation = sensorLocations[0];
            }
            else
            {
                _sensorLocation = sensorLocations[1];
            }

            await InitializeCamera(_sensorLocation);

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device != null && _dataContext.Device.SensorLocation == CameraSensorLocation.Back ?
                           _dataContext.Device.SensorRotationInDegrees :
                         - _dataContext.Device.SensorRotationInDegrees
            };

            videoBrush.SetSource(_dataContext.Device);
            videoBrush.Opacity = 1;

            overlayComboBox.Opacity = 1;

            HideProgress();

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);
        }

        /// <summary>
        /// Clicking on the capture button initiates autofocus and captures a photo.
        /// </summary>
        private async void captureButton_Click(object sender, EventArgs e)
        {
            if (!_manuallyFocused)
            {
                await AutoFocus();
            }

            await Capture();
        }

        /// <summary>
        /// Clicking on the about menu item initiates navigating to the about page.
        /// </summary>
        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Set autofocus area to tap location and refocus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void videoCanvas_Tap(object sender, GestureEventArgs e)
        {
            System.Windows.Point uiTapPoint = e.GetPosition(VideoCanvas);

            if (PhotoCaptureDevice.IsFocusRegionSupported(_dataContext.Device.SensorLocation) && _focusSemaphore.WaitOne(0))
            {
                // Get tap coordinates as a foundation point
                Windows.Foundation.Point tapPoint = new Windows.Foundation.Point(uiTapPoint.X, uiTapPoint.Y);

                double xRatio = VideoCanvas.ActualHeight / _dataContext.Device.PreviewResolution.Width;
                double yRatio = VideoCanvas.ActualWidth / _dataContext.Device.PreviewResolution.Height;

                // adjust to center focus on the tap point
                Windows.Foundation.Point displayOrigin = new Windows.Foundation.Point(
                            tapPoint.Y - _focusRegionSize.Width / 2,
                            (VideoCanvas.ActualWidth - tapPoint.X) - _focusRegionSize.Height / 2);

                // adjust for resolution difference between preview image and the canvas
                Windows.Foundation.Point viewFinderOrigin = new Windows.Foundation.Point(displayOrigin.X / xRatio, displayOrigin.Y / yRatio);
                Windows.Foundation.Rect focusrect = new Windows.Foundation.Rect(viewFinderOrigin, _focusRegionSize);

                // clip to preview resolution
                Windows.Foundation.Rect viewPortRect = new Windows.Foundation.Rect(0, 0, _dataContext.Device.PreviewResolution.Width, _dataContext.Device.PreviewResolution.Height);
                focusrect.Intersect(viewPortRect);

                _dataContext.Device.FocusRegion = focusrect;

                // show a focus indicator
                FocusIndicator.SetValue(Shape.StrokeProperty, _notFocusedBrush);
                FocusIndicator.SetValue(Canvas.LeftProperty, uiTapPoint.X - _focusRegionSize.Width / 2);
                FocusIndicator.SetValue(Canvas.TopProperty, uiTapPoint.Y - _focusRegionSize.Height / 2);
                FocusIndicator.SetValue(Canvas.VisibilityProperty, Visibility.Visible);

                CameraFocusStatus status = await _dataContext.Device.FocusAsync();

                if (status == CameraFocusStatus.Locked)
                {
                    FocusIndicator.SetValue(Shape.StrokeProperty, _focusedBrush);
                    _manuallyFocused = true;
                    _dataContext.Device.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters,
                        AutoFocusParameters.Exposure & AutoFocusParameters.Focus & AutoFocusParameters.WhiteBalance);
                }
                else
                {
                    _manuallyFocused = false;
                    _dataContext.Device.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);
                }

                _focusSemaphore.Release();
            }
        }

        /// <summary>
        /// Starts displaying progress indicator.
        /// </summary>
        /// <param name="msg">Text message to display.</param>
        private void ShowProgress(String msg)
        {
            _progressIndicator.Text = msg;
            _progressIndicator.IsVisible = true;

            SystemTray.SetProgressIndicator(this, _progressIndicator);
        }

        /// <summary>
        /// Stops displaying progress indicator.
        /// </summary>
        private void HideProgress()
        {
            _progressIndicator.IsVisible = false;

            SystemTray.SetProgressIndicator(this, _progressIndicator);
        }

        /// <summary>
        /// Initializes camera. Once initialized the instance is set to the
        /// DataContext.Device property for further usage from this or other
        /// pages.
        /// </summary>
        /// <param name="sensorLocation">Camera sensor to initialize.</param>
        private async Task InitializeCamera(CameraSensorLocation sensorLocation)
        {
            // Find out the largest capture resolution available on device
            IReadOnlyList<Windows.Foundation.Size> availableResolutions =
                PhotoCaptureDevice.GetAvailableCaptureResolutions(sensorLocation);

            Windows.Foundation.Size captureResolution = new Windows.Foundation.Size(0, 0);

            for (int i = 0; i < availableResolutions.Count; ++i)
            {
                if (captureResolution.Width < availableResolutions[i].Width)
                {
                    Debug.WriteLine("MainPage.InitializeCamera(): New capture resolution: " + availableResolutions[i]);
                    captureResolution = availableResolutions[i];
                }
            }

            PhotoCaptureDevice device =
                await PhotoCaptureDevice.OpenAsync(sensorLocation, DefaultCameraResolution);

            await device.SetPreviewResolutionAsync(DefaultCameraResolution);
            await device.SetCaptureResolutionAsync(captureResolution);

            device.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation,
                          device.SensorLocation == CameraSensorLocation.Back ?
                          device.SensorRotationInDegrees : - device.SensorRotationInDegrees);

            _dataContext.Device = device;
        }

        /// <summary>
        /// Starts autofocusing, if supported. Capturing buttons are disabled while focusing.
        /// </summary>
        private async Task AutoFocus()
        {
            if (!_capturing && PhotoCaptureDevice.IsFocusSupported(_dataContext.Device.SensorLocation))
            {
                SetScreenButtonsEnabled(false);
                SetCameraButtonsEnabled(false);

                await _dataContext.Device.FocusAsync();

                SetScreenButtonsEnabled(true);
                SetCameraButtonsEnabled(true);

                _capturing = false;
            }
        }

        /// <summary>
        /// Captures a photo. Photo data is stored to DataContext.ImageStream, and application
        /// is navigated to the preview page after capturing.
        /// </summary>
        private async Task Capture()
        {
            bool goToPreview = false;

            if (!_capturing)
            {
                _capturing = true;

                MemoryStream stream = new MemoryStream();

                CameraCaptureSequence sequence = _dataContext.Device.CreateCaptureSequence(1);
                sequence.Frames[0].CaptureStream = stream.AsOutputStream();

                await _dataContext.Device.PrepareCaptureSequenceAsync(sequence);
                await sequence.StartCaptureAsync();

                _dataContext.ImageStream = stream;

                _capturing = false;

                // Defer navigation as it will release the camera device and the
				// following Device calls must still work.
                goToPreview = true;
            }
			
            _manuallyFocused = false;

            if (PhotoCaptureDevice.IsFocusRegionSupported(_dataContext.Device.SensorLocation))
            {
                _dataContext.Device.FocusRegion = null;
            }

            FocusIndicator.SetValue(Canvas.VisibilityProperty, Visibility.Collapsed);
            _dataContext.Device.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);

            if (goToPreview)
            {
                NavigationService.Navigate(new Uri("/PreviewPage.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// Half-pressing the shutter key initiates autofocus.
        /// </summary>
        private async void ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            if (_manuallyFocused)
            {
                _manuallyFocused = false;
            }

            FocusIndicator.SetValue(Canvas.VisibilityProperty, Visibility.Collapsed);
            await AutoFocus();
        }

        /// <summary>
        /// Completely pressing the shutter key initiates capturing a photo.
        /// </summary>
        private async void ShutterKeyPressed(object sender, EventArgs e)
        {
            await Capture();
        }

        /// <summary>
        /// Creates an application bar based on the amount of sensors.
        /// </summary>
        private void CreateAppBar()
        {
            ApplicationBar appBar = new ApplicationBar();

            if (PhotoCaptureDevice.AvailableSensorLocations.Count > 1)
            {
                _sensorButton = new ApplicationBarIconButton(new Uri("Assets/Icons/appbar.sensor.png", UriKind.Relative));
                _sensorButton.Click += new EventHandler(sensorButton_Click);
                _sensorButton.Text = "sensor";
                _sensorButton.IsEnabled = false;
                appBar.Buttons.Add(_sensorButton);
            }

            _captureButton = new ApplicationBarIconButton(new Uri("Assets/Icons/appbar.feature.camera.rest.png", UriKind.Relative));
            _captureButton.Click += new EventHandler(captureButton_Click);
            _captureButton.Text = "capture";
            _captureButton.IsEnabled = false;
            appBar.Buttons.Add(_captureButton);

            _settingsButton = new ApplicationBarIconButton(new Uri("Assets/Icons/appbar.feature.settings.rest.png", UriKind.Relative));
            _settingsButton.Click += new EventHandler(settingsButton_Click);
            _settingsButton.Text = "settings";
            _settingsButton.IsEnabled = false;
            appBar.Buttons.Add(_settingsButton);

            ApplicationBar = appBar;
        }
    }
}