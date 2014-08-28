using CameraExplorer.Models;
using CameraExplorer.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace CameraExplorer
{

    enum CameraSessionState
    { 
        Init,
        Preview,
        Capture,
        Closed
    }

    class CameraSession
    {

        private volatile static CameraSession instance;
        private static object syncRoot = new Object();

        public MediaCapture MediaCapture { get; private set; }
        public DeviceInformationCollection Cameras { get; private set; }
        public CameraSettings Settings { get; set; }
        public CameraSessionState State { get; set; }

        private CaptureElement _viewFinder;

        private string _deviceId;

        public event EventHandler StateChanged;

        public CameraSession()
        {
            MediaCapture = new MediaCapture();
            Settings = new CameraSettings();            
            Settings.PropertyChanged += Settings_PropertyChanged;
            StateChanged += CameraSession_StateChanged;
        }

        void CameraSession_StateChanged(object sender, EventArgs e)
        {
            // dummy
        }

        public static CameraSession Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CameraSession();
                    }
                }
                return instance;
            }
        }

        public async Task Open()
        {
            System.Diagnostics.Debug.WriteLine("Opening session");
            SetState(CameraSessionState.Init);

            if (MediaCapture == null)
            {
                MediaCapture = new MediaCapture();
            }

            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            settings.PhotoCaptureSource = PhotoCaptureSource.Photo;
            if (_deviceId != null)
            {
                settings.VideoDeviceId = _deviceId;
            }
            await MediaCapture.InitializeAsync(settings);

            if (_deviceId == null)
            {
                Cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                _deviceId = MediaCapture.MediaCaptureSettings.VideoDeviceId;
            }

            Settings.CopyFrom(MediaCapture);

            SetState(CameraSessionState.Preview);

            if (_viewFinder != null) {
                _viewFinder.Source = MediaCapture;
                await MediaCapture.StartPreviewAsync();
            }

            System.Diagnostics.Debug.WriteLine("Opened");
        }

        public async Task Close()
        {
            System.Diagnostics.Debug.WriteLine("Closing session");
            if (_viewFinder != null)
            {
                await MediaCapture.StopPreviewAsync();
            }
    
            if (MediaCapture != null)
            {
                MediaCapture.Dispose();
                MediaCapture = null;
            }

            SetState(CameraSessionState.Closed);
        }

        private void SetState(CameraSessionState state)
        {
            State = state;
            StateChanged(this, new StateChangedEventArgs(state));
        }

        public async Task SetCaptureResolution(IMediaEncodingProperties resolution)
        {
            await MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolution);
            await SetPreviewResolutionForCaptureResolution(resolution);
        }

        private async Task SetPreviewResolutionForCaptureResolution(IMediaEncodingProperties resolution)
        {
            double aspectRatio = 3.0 / 4.0;

            if (resolution.GetType() == typeof(VideoEncodingProperties))
            {
                VideoEncodingProperties vep = ((VideoEncodingProperties)resolution);
                aspectRatio = (double)vep.Width / vep.Height;
            }
            else
            {
                ImageEncodingProperties iep = ((ImageEncodingProperties)resolution);
                aspectRatio = (double)iep.Width / iep.Height;
            }

            var properties = CameraSession.Instance.MediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);

            VideoEncodingProperties best = null;
            foreach (var p in properties)
            {
                VideoEncodingProperties vp = (VideoEncodingProperties)p;

                if (vp == null)
                    continue;

                double previewAspectRatio = (double)vp.Width / vp.Height;

                System.Diagnostics.Debug.WriteLine(previewAspectRatio + " and " + aspectRatio);

                // Find largest preview resolution matching the aspect ratio of capture resolution
                if (Math.Abs(aspectRatio - previewAspectRatio) < 0.001)
                    
                {
                    if (best == null ||
                        ((vp.Width * vp.Height) > (best.Width * best.Height)))
                    best = vp;
                }
            }

            if (best != null)
                await CameraSession.Instance.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, best);

        }

        public async Task Capture(IRandomAccessStream stream, PhotoOrientation orientation)
        {
            SetState(CameraSessionState.Capture);

            await CameraSession.Instance.MediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
            await CameraSession.Instance.MediaCapture.VideoDeviceController.RegionsOfInterestControl.ClearRegionsAsync();

            SetState(CameraSessionState.Preview);
            
            var bitmapDecoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, stream);
            var bitmapEncoder = await BitmapEncoder.CreateForInPlacePropertyEncodingAsync(bitmapDecoder);
            var orientationValue = new Windows.Graphics.Imaging.BitmapTypedValue(
                orientation,
                Windows.Foundation.PropertyType.UInt16
                );
            var retrievedPropertyTypes = new List<string> { "System.Photo.Orientation" };
            var properties = await bitmapEncoder.BitmapProperties.GetPropertiesAsync(retrievedPropertyTypes);
            properties["System.Photo.Orientation"] = orientationValue;
            await bitmapEncoder.BitmapProperties.SetPropertiesAsync(properties);
            await bitmapEncoder.FlushAsync();
        }

        public async void SetViewfinder(CaptureElement captureElement)
        {
            if (_viewFinder == captureElement)
                return;

            if (_viewFinder != null)
                await MediaCapture.StopPreviewAsync();

            _viewFinder = captureElement;

            if (MediaCapture != null)
            {
                captureElement.Source = MediaCapture;
                await MediaCapture.StartPreviewAsync();
            }
        }

        public async Task NextDevice()
        {
            for (int i = 0; i < Cameras.Count; i++)
            {
                // ToLower() is a workaround for a bug where Lumia 1520 default camera ID is in lower case
                if (Cameras[i].Id.ToLower() == MediaCapture.MediaCaptureSettings.VideoDeviceId.ToLower())
                {
                    if ((i + 1) >= Cameras.Count)
                    {
                        _deviceId = Cameras[i - Cameras.Count + 1].Id;
                    }
                    else
                    {
                        _deviceId = Cameras[i + 1].Id;
                    }
                }
            }

            await Close();

            await Open();
        }

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Setting changed! " + e.PropertyName);
            switch (e.PropertyName)
            {
                case CameraSettings.PROPERTY_ZOOM:
                    MediaCapture.VideoDeviceController.Zoom.TrySetValue(Settings.Zoom);
                    break;

                case CameraSettings.PROPERTY_FLASH_MODE:
                    Settings_OnFlashModeChange(Settings.FlashMode);
                    break;

                case CameraSettings.PROPERTY_CAPTURE_SCENE_MODE:
                    System.Diagnostics.Debug.WriteLine("Scene mode: " + Settings.CaptureSceneMode);
                    await MediaCapture.VideoDeviceController.SceneModeControl.SetValueAsync(Settings.CaptureSceneMode);
                    Settings.CopyFrom(MediaCapture);
                    break;

                case CameraSettings.PROPERTY_ENCODING:
                    await SetCaptureResolution(Settings.Encoding);
                    break;
                case CameraSettings.PROPERTY_EXPOSURE_TIME:
                    await MediaCapture.VideoDeviceController.ExposureControl.SetValueAsync(new TimeSpan(0, 0, 0, 0, (int)Settings.ExposureTime));
                    break;

                case CameraSettings.PROPERTY_EXPOSURE_COMPENSATION:
                    await MediaCapture.VideoDeviceController.ExposureCompensationControl.SetValueAsync((float) Settings.ExposureCompensation);
                    break;

                case CameraSettings.PROPERTY_FOCUS_MODE:
                    FocusSettings fs = new FocusSettings();
                    fs.Mode = Settings.FocusMode;

                    break;

                case CameraSettings.PROPERTY_HUE:
                    System.Diagnostics.Debug.WriteLine(Settings.Hue);
                    MediaCapture.VideoDeviceController.Hue.TrySetValue(Settings.Hue);
                    break;

            }
        }

        private void Settings_OnFlashModeChange(FlashMode e)
        {
            switch (e)
            {
                case FlashMode.Auto:
                    MediaCapture.VideoDeviceController.FlashControl.Auto = true;
                    break;
                case FlashMode.Off:
                    MediaCapture.VideoDeviceController.FlashControl.Auto = false;
                    MediaCapture.VideoDeviceController.FlashControl.Enabled = false;
                    break;
                case FlashMode.On:
                    MediaCapture.VideoDeviceController.FlashControl.Auto = false;
                    MediaCapture.VideoDeviceController.FlashControl.Enabled = true;
                    break;
            }
        }

        public class StateChangedEventArgs : EventArgs
        {
            public CameraSessionState State { get; set; }

            public StateChangedEventArgs(CameraSessionState state)
            {
                State = state;
            }
        }

    }
}
