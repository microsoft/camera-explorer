using System;
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

        public void CreateParameters()
        {
            _parameters.Clear();

            if (_dataContext.Device != null)
            {
                ObservableCollection<Parameter> ps = new ObservableCollection<Parameter>();

                ps.Add(new SceneModeParameter(_dataContext.Device));
                ps.Add(new WhiteBalancePresetParameter(_dataContext.Device));

                // ps.Add(new PreviewResolutionParameter(_dataContext.Device)); // todo throws exception when setting this
                ps.Add(new CaptureResolutionParameter(_dataContext.Device));

                //ps.Add(new FlashModeParameter(_dataContext.Device)); // todo does not capture after setting this
                ps.Add(new FlashPowerParameter(_dataContext.Device));
                ps.Add(new IsoParameter(_dataContext.Device));
                //ps.Add(new ExposureCompensationParameter(_dataContext.Device)); // todo does not work, does not capture after setting this
                //ps.Add(new ManualWhiteBalanceParameter(_dataContext.Device)); // todo dependency with wb preset
                ps.Add(new ExposureTimeParameter(_dataContext.Device));
                ps.Add(new AutoFocusRangeParameter(_dataContext.Device));
                ps.Add(new FocusIlluminationModeParameter(_dataContext.Device));

                for (int i = 0; i < ps.Count;)
                {
                    Parameter p = ps[i];

                    if (p.Supported && p.Modifiable)
                    {
                        try
                        {
                            p.SetDefault();
                        }
                        catch (Exception)
                        {
                            System.Diagnostics.Debug.WriteLine("Setting default to " + p.Name.ToLower() + " failed");
                        }

                        i++;
                    }
                    else
                    {
                        ps.RemoveAt(i);
                    }
                }

                foreach (Parameter p in ps)
                {
                    p.Refresh();
                }

                _parameters = ps;
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
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