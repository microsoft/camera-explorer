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
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class Parameter
        {
            public string DisplayName { get; set; }
        }

        public class RangeParameter : Parameter, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            object _min;
            public object Min
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

            object _max;
            public object Max
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

            object _currentValue;
            public object CurrentValue
            {
                get {
                    if (_currentValue != null)
                    {
                        return _currentValue;
                    }
                    else
                    {
                        return _min;
                    }
                }

                set
                {
                    if (_currentValue != value)
                    {
                        Type t;

                        if (_currentValue != null)
                        {
                            t = _currentValue.GetType();
                        }
                        else if (_min != null)
                        {
                            t = _min.GetType();
                        }
                        else if (_max != null)
                        {
                            t = _max.GetType();
                        }
                        else
                        {
                            t = null;
                        }

                        if (t != null)
                        {
                            _currentValue = Convert.ChangeType(value, t);
                        }
                        else
                        {
                            _currentValue = value;
                        }

                        if (Action != null)
                        {
                            Action();
                        }

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("CurrentValue"));
                            PropertyChanged(this, new PropertyChangedEventArgs("CurrentValueAsString"));
                        }
                    }
                }
            }

            public string CurrentValueAsString
            {
                get
                {
                    if (_currentValue != null)
                    {
                        if (Unit != "")
                        {
                            return _currentValue.ToString() + " " + Unit;
                        }
                        else
                        {
                            return _currentValue.ToString();
                        }
                    }
                    else
                    {
                        return "Not set";
                    }
                }
            }


            public string Unit { get; set; }
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
                if (_parameters != null)
                {
                    _parameters = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
                    }
                }
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

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Device"));
                    }
                }
            }
        }

        public void Refresh()
        {
            int count = _parameters.Count;

            _parameters.Clear();

            if (_device != null)
            {
                PhotoCaptureDeviceParser.Parse(_device, _parameters);
            }

            if ((count > 0 || _parameters.Count > 0) && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Parameters"));
            }
        }
    }
}