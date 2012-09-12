using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class DataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static DataContext _singleton;
        public static CameraExplorer.DataContext Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new CameraExplorer.DataContext();

                return _singleton;
            }
        }

        Settings _settings = null;
        public Settings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new Settings();

                return _settings;
            }
        }

        PhotoCaptureDevice _device = null;
        public PhotoCaptureDevice Device
        {
            get
            {
                return _device;
            }

            set
            {
                if (_device != value)
                {
                    _device = value;

                    Settings.Refresh();

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Device"));
                    }
                }
            }
        }

        BitmapImage  _image = new BitmapImage();
        public BitmapImage Image
        {
            get
            {
                return _image;
            }
        }

        public async Task InitializeCamera(CameraSensorLocation sensorLocation)
        {
            Windows.Foundation.Size initialResolution = new Windows.Foundation.Size(640, 480);
            Windows.Foundation.Size previewResolution = new Windows.Foundation.Size(640, 480);
            Windows.Foundation.Size captureResolution = new Windows.Foundation.Size(640, 480);

            PhotoCaptureDevice d = await PhotoCaptureDevice.OpenAsync(sensorLocation, initialResolution);

            await d.SetPreviewResolutionAsync(previewResolution);
            await d.SetCaptureResolutionAsync(captureResolution);

            d.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, d.SensorRotationInDegrees);

            Device = d;
        }

        public void UnitializeCamera()
        {
            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }
        }
    }
}
