using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;
        private ObservableCollection<Parameter> _parameters = new ObservableCollection<Parameter>();

        public ObservableCollection<Parameter> Parameters
        {
            get
            {
                return _parameters;
            }

            private set
            {
                if (_parameters != value)
                {
                    _parameters = value;

                    PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
                }
            }
        }

        public void CreateParameters()
        {
            if (_dataContext.Device != null)
            {
                ObservableCollection<Parameter> newParameters = new ObservableCollection<Parameter>();

                Action<Parameter> addParameter = (Parameter parameter) =>
                    {
                        if (parameter.Supported && parameter.Modifiable)
                        {
                            try
                            {
                                parameter.Refresh();
                                parameter.SetDefault();
                                parameter.Refresh();

                                newParameters.Add(parameter);
                            }
                            catch (Exception)
                            {
                                System.Diagnostics.Debug.WriteLine("Setting default to " + parameter.Name.ToLower() + " failed");
                            }
                        }
                    };

                addParameter(new SceneModeParameter(_dataContext.Device));
                addParameter(new WhiteBalancePresetParameter(_dataContext.Device));
                //addParameter(new PreviewResolutionParameter(_dataContext.Device)); // todo throws exception when setting this
                //addParameter(new CaptureResolutionParameter(_dataContext.Device)); // todo does not capture after setting this
                //addParameter(new FlashModeParameter(_dataContext.Device)); // todo does not capture after setting this
                addParameter(new FlashPowerParameter(_dataContext.Device));
                addParameter(new IsoParameter(_dataContext.Device));
                //addParameter(new ExposureCompensationParameter(_dataContext.Device)); // todo does not work, does not capture after setting this
                //addParameter(new ManualWhiteBalanceParameter(_dataContext.Device)); // todo dependency with wb preset
                addParameter(new ExposureTimeParameter(_dataContext.Device));
                addParameter(new AutoFocusRangeParameter(_dataContext.Device));
                addParameter(new FocusIlluminationModeParameter(_dataContext.Device));

                Parameters = newParameters;
            }
        }

        public void Refresh()
        {
            foreach (Parameter p in _parameters)
            {
                p.Refresh();
            }
        }
    }
}