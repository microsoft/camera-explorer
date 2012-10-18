using System.ComponentModel;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    public abstract class Parameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PhotoCaptureDevice _device;
        private string _name;
        private string _overlaySource;
        private bool _supported = true;
        private bool _modifiable = true;

        protected Parameter(PhotoCaptureDevice device, string name)
        {
            _device = device;
            _name = name;
        }

        protected void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public PhotoCaptureDevice Device
        {
            get
            {
                return _device;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string OverlaySource
        {
            get
            {
                return _overlaySource;
            }

            protected set
            {
                if (_overlaySource != value)
                {
                    _overlaySource = value;

                    NotifyPropertyChanged("OverlaySource");
                }
            }
        }

        public bool Supported
        {
            get
            {
                return _supported;
            }

            protected set
            {
                if (_supported != value)
                {
                    _supported = value;

                    NotifyPropertyChanged("Supported");
                }
            }
        }

        public bool Modifiable
        {
            get
            {
                return _modifiable;
            }

            protected set
            {
                if (_modifiable != value)
                {
                    _modifiable = value;

                    NotifyPropertyChanged("Modifiable");
                }
            }
        }

        public abstract void Refresh();

        public abstract void SetDefault();
    }
}