using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;

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

        public void Refresh()
        {
            _parameters.Clear();

            if (_dataContext.Device != null)
            {
                _parameters.Add(new SceneModeParameter(_dataContext.Device));
                _parameters.Add(new WhiteBalancePresetParameter(_dataContext.Device));

                _parameters.Add(new PreviewResolutionParameter(_dataContext.Device));
                _parameters.Add(new CaptureResolutionParameter(_dataContext.Device));

                _parameters.Add(new FlashModeParameter(_dataContext.Device));
                _parameters.Add(new FlashPowerParameter(_dataContext.Device));
                _parameters.Add(new IsoParameter(_dataContext.Device));
                _parameters.Add(new ExposureCompensationParameter(_dataContext.Device));
                _parameters.Add(new ManualWhiteBalanceParameter(_dataContext.Device));
                _parameters.Add(new ExposureTimeParameter(_dataContext.Device)); // problems
                _parameters.Add(new AutoFocusRangeParameter(_dataContext.Device));
                _parameters.Add(new FocusIlluminationModeParameter(_dataContext.Device));
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
        }
    }
}