using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class DataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static DataContext _singleton;
        private PhotoCaptureDevice _device = null;
        private ObservableCollection<Parameter> _parameters = new ObservableCollection<Parameter>();

        public static DataContext Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new DataContext();
                }

                return _singleton;
            }
        }

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


                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
                    }
                }
            }
        }

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
                    
                    if (_device != null)
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

                                    newParameters.Add(parameter);
                                }
                                catch (Exception)
                                {
                                    System.Diagnostics.Debug.WriteLine("Setting default to " + parameter.Name.ToLower() + " failed");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Parameter " + parameter.Name.ToLower() + " is not supported or not modifiable");
                            }
                        };

                        addParameter(new SceneModeParameter(_device));
                        addParameter(new WhiteBalancePresetParameter(_device));
                        addParameter(new FlashModeParameter(_device));
                        addParameter(new FlashPowerParameter(_device));
                        addParameter(new IsoParameter(_device));
                        addParameter(new ExposureCompensationParameter(_device));
                        addParameter(new ExposureTimeParameter(_device));
                        addParameter(new AutoFocusRangeParameter(_device));
                        addParameter(new FocusIlluminationModeParameter(_device));
                        addParameter(new CaptureResolutionParameter(_device));

                        Parameters = newParameters;
                    }

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Device"));
                    }
                }
            }
        }

        public MemoryStream ImageStream { get; set; }
    }
}