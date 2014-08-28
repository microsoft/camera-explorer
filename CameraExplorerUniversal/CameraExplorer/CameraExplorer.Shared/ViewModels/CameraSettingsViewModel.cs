using CameraExplorer.Models;
using CameraExplorer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using System.Linq;

namespace CameraExplorer.ViewModels
{
    class CameraSettingsViewModel : INotifyPropertyChanged
    {
        public IReadOnlyList<IMediaEncodingProperties> Encodings { get; private set; }
        public IEnumerable<CaptureSceneMode> CaptureSceneModes { get; private set; }
        public IReadOnlyList<FlashMode> FlashModes { get; private set; }
        public IReadOnlyList<FocusMode> FocusModes { get; private set; }
        //public IEnumerable<>
        public IEnumerable<IsoSpeedPreset> IsoSpeedPresets { get; private set; }
        public IEnumerable<ColorTemperaturePreset> ColorTemperaturePresets { get; private set; }
        public FocusControl FocusRangeControl { get; private set; }
        public WhiteBalanceControl ColorTemperatureControl { get; private set; }
        public BoundedRangeModel ExposureTimeControl { get; private set; }
        public ExposureCompensationControl ExposureCompensationControl { get; private set; }
        public BoundedRangeModel HueControl { get; private set; }
        public BoundedRangeModel PanControl { get; private set; }
        public BoundedRangeModel ZoomControl { get; private set; }

        public bool FlashSupported { get; private set; }
        public bool FocusSupported { get; private set; }
        public bool AutoFocusSupported { get; private set; }
        public bool ZoomSupported { get; private set; }
        public bool HueSupported { get; private set; }
        public bool PanSupported { get; private set; }
        public bool ColorTemperatureSupported { get; private set; }
        public bool IsoSpeedSupported { get; private set; }
        public bool ExposureTimeSupported { get; private set; }
        public bool ExposureCompensationSupported { get; private set; }

        public CameraSettings Settings { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public CameraSettingsViewModel(MediaCapture mediaCapture, CameraSettings settings)
        {
            Settings = settings;
            CopyFrom(mediaCapture);
        }

        public void CopyFrom(MediaCapture mediaCapture)
        {

            var videoDeviceController = mediaCapture.VideoDeviceController;
            Encodings = CameraUtilities.GetAvailableCaptureResolutions(mediaCapture);      
            CaptureSceneModes = videoDeviceController.SceneModeControl.SupportedModes.ToArray();
            //CaptureSceneModes = (IEnumerable<CaptureSceneMode>)Enum.GetValues(typeof(CaptureSceneMode));
            ColorTemperaturePresets = (IEnumerable<ColorTemperaturePreset>) Enum.GetValues(typeof(ColorTemperaturePreset));
            IsoSpeedPresets = videoDeviceController.IsoSpeedControl.SupportedPresets.ToArray();
            FocusRangeControl = videoDeviceController.FocusControl;
            ColorTemperatureControl = videoDeviceController.WhiteBalanceControl;
            ExposureCompensationControl = videoDeviceController.ExposureCompensationControl;
            ExposureTimeControl = new BoundedRangeModel(videoDeviceController.ExposureControl.Min.TotalMilliseconds, videoDeviceController.ExposureControl.Max.TotalMilliseconds, videoDeviceController.ExposureControl.Step.TotalMilliseconds);
            FocusModes = videoDeviceController.FocusControl.SupportedFocusModes;

            HueControl = new BoundedRangeModel(videoDeviceController.Hue.Capabilities.Min, videoDeviceController.Hue.Capabilities.Max, videoDeviceController.Hue.Capabilities.Step);
            PanControl = new BoundedRangeModel(videoDeviceController.Pan.Capabilities.Min, videoDeviceController.Pan.Capabilities.Max, videoDeviceController.Pan.Capabilities.Step);
            ZoomControl = new BoundedRangeModel(videoDeviceController.Zoom.Capabilities.Min, videoDeviceController.Zoom.Capabilities.Max, videoDeviceController.Zoom.Capabilities.Step);

            FlashSupported = videoDeviceController.FlashControl.Supported;
            FocusSupported = videoDeviceController.FocusControl.Supported;
            ZoomSupported = videoDeviceController.Zoom.Capabilities.Supported;
            ColorTemperatureSupported = videoDeviceController.WhiteBalanceControl.Supported;
            IsoSpeedSupported = mediaCapture.VideoDeviceController.IsoSpeedControl.Supported;
            ExposureTimeSupported = mediaCapture.VideoDeviceController.ExposureControl.Supported;
            ExposureCompensationSupported = mediaCapture.VideoDeviceController.ExposureCompensationControl.Supported;
            PanSupported = videoDeviceController.Pan.Capabilities.Supported;
            HueSupported = videoDeviceController.Hue.Capabilities.Supported;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class BoundedRangeModel
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Step { get; set; }

        public BoundedRangeModel(double min, double max, double step)
        {
            this.Min = min;
            this.Max = max;
            this.Step = step;
        }
    }

}
