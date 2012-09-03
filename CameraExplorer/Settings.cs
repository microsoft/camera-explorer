using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    class Settings
    {
        public class Parameter
        {
            public string DisplayName { get; set; }
        }

        public class RangeParameter : Parameter, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            int _min;
            public int Min
            {
                get
                {
                    return _min;
                }

                set
                {
                    if (_min != value)
                    {
                        _min = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Min"));
                        }
                    }
                }
            }

            int _max;
            public int Max
            {
                get
                {
                    return _max;
                }

                set
                {
                    if (_max != value)
                    {
                        _max = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Max"));
                        }
                    }
                }
            }

            public Action Action { get; set; }

            int _currentValue;
            public int CurrentValue
            {
                get {
                    return _currentValue;
                }

                set
                {
                    if (_currentValue != value)
                    {
                        _currentValue = value;

                        if (Action != null)
                        {
                            Action();
                        }

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("CurrentValue"));
                        }
                    }
                }
            }

            public string UnitDisplayName { get; set; }
        }

        public class ArrayParameter : Parameter, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public class Value
            {
                public string DisplayName { get; set; }
                public Action Action { get; set; }
            }

            ObservableCollection<Value> _possibleValues = new ObservableCollection<Value>();
            public ObservableCollection<Value> PossibleValues
            {
                get
                {
                    return _possibleValues;
                }

                set
                {
                    _possibleValues = value;
                }
            }

            Value _currentValue = null;
            public Value CurrentValue
            {
                get
                {
                    return _currentValue;
                }

                set
                {
                    if (_currentValue != value)
                    {
                        _currentValue = value;

                        if (_currentValue != null && _currentValue.Action != null)
                        {
                            _currentValue.Action();
                        }

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("CurrentValue"));
                        }
                    }
                }
            }
        }

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
                    _parameters.Clear();

                    if (_device != null)
                    {
                        PhotoCaptureDeviceParser.Parse(_device, _parameters);
                    }
                }
            }
        }
    }
}
