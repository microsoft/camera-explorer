using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CameraExplorer.Resources;
using Microsoft.Devices;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace CameraExplorer
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton();

        public MainPage()
        {
            InitializeComponent();

            DataContext = _dataContext;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_dataContext.Device == null)
            {
                ApplicationBar.IsVisible = false;

                await _dataContext.InitializeCamera(CameraSensorLocation.Back);

                ApplicationBar.IsVisible = true;
            }

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device.SensorRotationInDegrees
            };

            videoBrush.SetSource(_dataContext.Device);
        }

        void PrintCameraParameters(PhotoCaptureDevice device)
        {
            PrintSensorLocations(device);
            PrintPreviewResolutions(device);
            PrintCaptureResolutions(device);
            PrintMiscellaneousInfo(device);
            
            PrintCameraParameter(KnownCameraPhotoProperties.ExposureCompensation, "KnownCameraPhotoProperties.ExposureCompensation");
            PrintCameraParameter(KnownCameraPhotoProperties.ExposureTime, "KnownCameraPhotoProperties.ExposureTime");
            PrintCameraParameter(KnownCameraPhotoProperties.FlashMode, "KnownCameraPhotoProperties.FlashMode");
            PrintCameraParameter(KnownCameraPhotoProperties.FlashPower, "KnownCameraPhotoProperties.FlashPower");
            PrintCameraParameter(KnownCameraPhotoProperties.FocusIlluminationMode, "KnownCameraPhotoProperties.FocusIlluminationMode");
            PrintCameraParameter(KnownCameraPhotoProperties.Iso, "KnownCameraPhotoProperties.Iso");
            PrintCameraParameter(KnownCameraPhotoProperties.LockedAutoFocusParameters, "KnownCameraPhotoProperties.LockedAutoFocusParameters");
            PrintCameraParameter(KnownCameraPhotoProperties.ManualWhiteBalance, "KnownCameraPhotoProperties.ManualWhiteBalance");
            PrintCameraParameter(KnownCameraPhotoProperties.SceneMode, "KnownCameraPhotoProperties.SceneMode");
            PrintCameraParameter(KnownCameraPhotoProperties.WhiteBalancePreset, "KnownCameraPhotoProperties.WhiteBalancePreset");

            PrintCameraParameter(KnownCameraGeneralProperties.AutoFocusRange, "KnownCameraGeneralProperties.AutoFocusRange");
            PrintCameraParameter(KnownCameraGeneralProperties.EncodeWithOrientation, "KnownCameraGeneralProperties.EncodeWithOrientation");
            PrintCameraParameter(KnownCameraGeneralProperties.IsShutterSoundEnabledByUser, "KnownCameraGeneralProperties.IsShutterSoundEnabledByUser");
            PrintCameraParameter(KnownCameraGeneralProperties.IsShutterSoundRequiredForRegion, "KnownCameraGeneralProperties.IsShutterSoundRequiredForRegion");
            PrintCameraParameter(KnownCameraGeneralProperties.ManualFocusPosition, "KnownCameraGeneralProperties.ManualFocusPosition");
            PrintCameraParameter(KnownCameraGeneralProperties.PlayShutterSoundOnCapture, "KnownCameraGeneralProperties.PlayShutterSoundOnCapture");
            PrintCameraParameter(KnownCameraGeneralProperties.SpecifiedCaptureOrientation, "KnownCameraGeneralProperties.SpecifiedCaptureOrientation");
        }

        void PrintMiscellaneousInfo(PhotoCaptureDevice device)
        {
            System.Diagnostics.Debug.WriteLine("PhotoCaptureDevice.IsFocusRegionSupported is " + PhotoCaptureDevice.IsFocusRegionSupported(device.SensorLocation));
            System.Diagnostics.Debug.WriteLine("");

            System.Diagnostics.Debug.WriteLine("PhotoCaptureDevice.IsFocusSupported is " + PhotoCaptureDevice.IsFocusSupported(device.SensorLocation));
            System.Diagnostics.Debug.WriteLine("");
        }

        void PrintPreviewResolutions(PhotoCaptureDevice device)
        {
            IReadOnlyList<Windows.Foundation.Size> resolutions = PhotoCaptureDevice.GetAvailablePreviewResolutions(device.SensorLocation);

            System.Diagnostics.Debug.WriteLine("Current preview resolution is " + device.PreviewResolution + ", all supported " + resolutions.Count + " resolutions being:");

            for (int i = 0; i < resolutions.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("    " + resolutions[i].Width + "x" + resolutions[i].Height);
            }

            System.Diagnostics.Debug.WriteLine("");
        }

        void PrintCaptureResolutions(PhotoCaptureDevice device)
        {
            IReadOnlyList<Windows.Foundation.Size> resolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(device.SensorLocation);

            System.Diagnostics.Debug.WriteLine("Current capture resolution is " + device.CaptureResolution + ", all supported " + resolutions.Count + " resolutions being:");

            for (int i = 0; i < resolutions.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("    " + resolutions[i].Width + "x" + resolutions[i].Height);
            }

            System.Diagnostics.Debug.WriteLine("");
        }

        void PrintSensorLocations(PhotoCaptureDevice device)
        {
            IReadOnlyList<CameraSensorLocation> sensorLocations = PhotoCaptureDevice.AvailableSensorLocations;

            System.Diagnostics.Debug.WriteLine("Current sensor location is " + device.SensorLocation + " with rotation of "
                + device.SensorRotationInDegrees + " degrees, all supported " + sensorLocations.Count + " sensor locations being:");

            for (int i = 0; i < sensorLocations.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("    " + sensorLocations[i].ToString());
            }

            System.Diagnostics.Debug.WriteLine("");
        }

        void PrintCameraParameter(Guid guid, string name)
        {
            bool isRange = true;
            object value = null;
            string valueString;

            try
            {
                CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(_dataContext.Device.SensorLocation, guid);
                value = _dataContext.Device.GetProperty(guid);
                valueString = value != null ? value.ToString() : "(null)";
                
                System.Diagnostics.Debug.WriteLine(name + " is a range " + range.Min + "..." + range.Max + " with current value " + valueString);
            }
            catch (Exception e)
            {
                isRange = false;
            }

            if (!isRange)
            {
                try
                {
                    IReadOnlyList<object> values = PhotoCaptureDevice.GetSupportedPropertyValues(_dataContext.Device.SensorLocation, guid);
                    value = _dataContext.Device.GetProperty(guid);
                    valueString = value != null ? value.ToString() : "(null)";

                    System.Diagnostics.Debug.WriteLine(name + " is a " + values.Count + "-value set with current value " + valueString + " and all values being:");

                    for (int i = 0; i < values.Count; i++)
                    {
                        System.Diagnostics.Debug.WriteLine("    " + values[i].ToString());
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to read " + name);
                }
            }

            System.Diagnostics.Debug.WriteLine("");
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            //PrintCameraParameters(_device);
        }

        private async void captureButton_Click(object sender, EventArgs e)
        {
            MemoryStream stream = new MemoryStream();

            CameraCaptureSequence sequence = _dataContext.Device.CreateCaptureSequence(1);
            sequence.Frames[0].CaptureStream = stream.AsOutputStream();

            await _dataContext.Device.PrepareCaptureSequenceAsync(sequence);
            await sequence.StartCaptureAsync();

            MediaLibrary library = new MediaLibrary();
            Picture picture = library.SavePictureToCameraRoll("Camera Explorer", stream);
        }
    }
}