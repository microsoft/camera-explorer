using Microsoft.Devices;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    public abstract class Parameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool _overlay;

        protected Parameter(PhotoCaptureDevice device, string name, bool overlay)
        {
            _device = device;
            _name = name;
            _overlay = overlay;
        }

        PhotoCaptureDevice _device;
        public PhotoCaptureDevice Device
        {
            get
            {
                return _device;
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            protected set
            {
                _name = value;

                NotifyPropertyChanged("Name");
            }
        }

        string _imageSource;
        public string ImageSource
        {
            get
            {
                if (_imageSource == null && _overlay)
                {
                    return "";
                }
                else
                {
                    return _imageSource;
                }
            }

            protected set
            {
                if (_overlay)
                {
                    if (value == null)
                    {
                        _imageSource = "";
                    }
                    else
                    {
                        _imageSource = value;
                    }

                    NotifyPropertyChanged("ImageSource");
                }
                else if (value != null)
                {
                    throw new ArgumentException("Overlays are not enabled for this parameter");
                }
            }
        }

        bool _supported = true;
        public bool Supported
        {
            get
            {
                return _supported;
            }

            protected set
            {
                _supported = value;

                NotifyPropertyChanged("Supported");
            }
        }

        bool _modifiable = true;
        public bool Modifiable
        {
            get
            {
                return _modifiable;
            }

            protected set
            {
                _modifiable = value;

                NotifyPropertyChanged("Modifiable");
            }
        }

        protected virtual void SetDefault()
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}