using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<Parameter> _parameters = new ObservableCollection<Parameter>();
        public ObservableCollection<Parameter> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
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

                    PropertyChanged(this, new PropertyChangedEventArgs("Device"));

                    Refresh();
                }
            }
        }

        public void Refresh()
        {
            _parameters.Clear();

            _parameters.Add(new IsoParameter(_device));
            _parameters.Add(new SceneModeParameter(_device));
            _parameters.Add(new FlashPowerParameter(_device));
            _parameters.Add(new ManualWhiteBalanceParameter(_device));
            _parameters.Add(new ExposureTimeParameter(_device));
            _parameters.Add(new FlashModeParameter(_device));
            _parameters.Add(new WhiteBalancePresetParameter(_device));
            _parameters.Add(new AutoFocusRangeParameter(_device));
            _parameters.Add(new FocusIlluminationModeParameter(_device));
            _parameters.Add(new PreviewResolutionParameter(_device));
            _parameters.Add(new CaptureResolutionParameter(_device));
            _parameters.Add(new ExposureCompensationParameter(_device));

            PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
        }
    }
}