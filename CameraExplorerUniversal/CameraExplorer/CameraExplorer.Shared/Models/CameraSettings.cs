using CameraExplorer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;

namespace CameraExplorer.Models
{

    enum FlashMode
    {
        Auto,
        Off,
        On
    }

    class CameraSettings : INotifyPropertyChanged
    {

        public const string PROPERTY_ZOOM = "Zoom";
        public const string PROPERTY_ENCODING = "CaptureEncoding";
        public const string PROPERTY_AUTO_EXPOSURE_ENABLED = "AutoExposure";
        public const string PROPERTY_EXPOSURE_TIME = "ExposureTime";
        public const string PROPERTY_EXPOSURE_COMPENSATION = "ExposureCompensation";
        public const string PROPERTY_ISO_SPEED = "IsoSpeed";
        public const string PROPERTY_CAPTURE_SCENE_MODE = "CaptureSceneMode";
        public const string PROPERTY_FLASH_MODE = "FlashMode";
        public const string PROPERTY_COLOR_TEMPERATURE_PRESET = "ColorTemperature";
        public const string PROPERTY_AUTO_FOCUS_ENABLED = "AutoFocusEnabled";
        public const string PROPERTY_FOCUS_RANGE = "FocusRange";
        public const string PROPERTY_FOCUS_MODE = "FocusMode";
        public const string PROPERTY_HUE = "Hue";

        public event PropertyChangedEventHandler PropertyChanged;

        private double _zoom = 1;
        public double Zoom { 
            get { return _zoom; }
            set { _zoom = value; OnPropertyChanged(PROPERTY_ZOOM); } 
        }

        private double _focusRange;
        public double FocusRange
        {
            get { return _focusRange; }
            set { _focusRange = value; OnPropertyChanged(PROPERTY_FOCUS_RANGE); }
        }

        private bool _autoFocusEnabled;
        public bool AutoFocusEnabled
        {
            get { return _autoFocusEnabled; }
            set { _autoFocusEnabled = value; OnPropertyChanged(PROPERTY_AUTO_FOCUS_ENABLED); }
        }

        private IsoSpeedPreset _isoSpeedPreset;
        public IsoSpeedPreset IsoSpeedPreset
        {
            get { return _isoSpeedPreset; }
            set { _isoSpeedPreset = value; OnPropertyChanged(PROPERTY_ISO_SPEED); }
        }

        private bool _autoExposureEnabled;
        public bool AutoExposureEnabled
        {
            get { return _autoExposureEnabled; }
            set { _autoExposureEnabled = value; OnPropertyChanged(PROPERTY_AUTO_EXPOSURE_ENABLED); }
        }

        private double _exposureTime;
        public double ExposureTime
        {
            get { return _exposureTime; }
            set { _exposureTime = value; OnPropertyChanged(PROPERTY_EXPOSURE_TIME); }
        }

        private double _exposureCompensation;
        public double ExposureCompensation
        {
            get { return _exposureCompensation; }
            set { _exposureCompensation = value; OnPropertyChanged(PROPERTY_EXPOSURE_COMPENSATION); }
        }

        private IMediaEncodingProperties _captureEncoding;
        public IMediaEncodingProperties Encoding
        {
            get { return _captureEncoding; }
            set { _captureEncoding = value; OnPropertyChanged(PROPERTY_ENCODING); }
        }

        private CaptureSceneMode _captureSceneMode;
        public CaptureSceneMode CaptureSceneMode
        {
            get { return _captureSceneMode; }
            set { _captureSceneMode = value; OnPropertyChanged(PROPERTY_CAPTURE_SCENE_MODE); }
        }

        private FlashMode _flashMode;
        public FlashMode FlashMode
        {
            get { return _flashMode; }
            set { _flashMode = value; OnPropertyChanged(PROPERTY_FLASH_MODE); }
        }

        private ColorTemperaturePreset _colorTemperaturePreset;
        public ColorTemperaturePreset ColorTemperaturePreset
        {
            get { return _colorTemperaturePreset; }
            set { _colorTemperaturePreset = value; OnPropertyChanged(PROPERTY_COLOR_TEMPERATURE_PRESET); }
        }

        private FocusMode _focusMode;
        public FocusMode FocusMode
        {
            get { return _focusMode; }
            set { _focusMode = value; OnPropertyChanged(PROPERTY_FOCUS_MODE); }
        }

        private double _hue;
        public double Hue
        {
            get { return _hue; }
            set { _hue = value; OnPropertyChanged(PROPERTY_HUE); }
        }

        public CameraSettings()
        {

        }

        public void CopyFrom(MediaCapture mediaCapture)
        {
            
            var videoDeviceController = mediaCapture.VideoDeviceController;
            
            _captureEncoding = CameraUtilities.GetCurrentCaptureResolution(mediaCapture);
            _captureSceneMode = videoDeviceController.SceneModeControl.Value;
            _colorTemperaturePreset = videoDeviceController.WhiteBalanceControl.Preset;
            
            _isoSpeedPreset = videoDeviceController.IsoSpeedControl.Preset;
            if (videoDeviceController.ExposureControl.Supported)
                videoDeviceController.Exposure.TryGetAuto(out _autoExposureEnabled);
            if (videoDeviceController.ExposureControl.Supported)
                _exposureTime = videoDeviceController.ExposureControl.Value.Milliseconds;
            
            if (videoDeviceController.ExposureCompensationControl.Supported)
                _exposureCompensation = videoDeviceController.ExposureCompensationControl.Value;
            /*if (videoDeviceController.Focus.Capabilities.Supported)
                _focusRange = videoDeviceController.FocusControl.Value;*/
            //_focusMode = videoDeviceController.FocusControl.Mode;
            //_autoFocusEnabled = videoDeviceController.FocusControl.Mode == FocusMode.Auto;
            //videoDeviceController.Hue.TryGetValue(out _hue);
        }

        public void NextFlashMode()
        {
            if ((int)FlashMode >= (Enum.GetValues(FlashMode.GetType()).Length - 1))
            {
                FlashMode = 0;
            }
            else 
            {
                FlashMode++;
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
