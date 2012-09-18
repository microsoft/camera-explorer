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
    public abstract class RangeParameter<T> : Parameter
    {
        Guid _guid;
        CameraCapturePropertyRange _range;

        protected RangeParameter(PhotoCaptureDevice device, Guid guid, string name, bool overlay = false)
            : base(device, name, overlay)
        {
            _guid = guid;

            Refresh();
        }

        void GetRange(ref CameraCapturePropertyRange range)
        {
            _range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, _guid);
        }

        void GetValue(ref T value)
        {
            _value = (T)Device.GetProperty(_guid);
        }

        public override void Refresh()
        {
            bool supported = false;

            try
            {
                GetRange(ref _range);

                supported = _range != null;

                if (supported)
                {
                    GetValue(ref _value);
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Getting " + Name.ToLower() + " failed");
            }

            Supported = supported;
            Modifiable = Supported && !_range.Min.Equals(_range.Max);

            if (supported)
            {
                NotifyPropertyChanged("Minimum");
                NotifyPropertyChanged("Maximum");
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("ImageSource");
            }
        }

        public T Minimum
        {
            get
            {
                return (T)_range.Min;
            }
        }

        public T Maximum
        {
            get
            {
                return (T)_range.Max;
            }
        }

        T _value;
        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                try
                {
                    Device.SetProperty(_guid, (T)value);

                    _value = value;

                    NotifyPropertyChanged("Value");
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("Setting " + Name.ToLower() + " failed");
                }
            }
        }
    }

    public class ExposureCompensationParameter : RangeParameter<Int32>
    {
        public ExposureCompensationParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ExposureCompensation, "Exposure compensation")
        {
        }

        public override void SetDefault()
        {
            Value = (Int32)Minimum + (Maximum - Minimum) / 2;
        }
    }

    public class ManualWhiteBalanceParameter : RangeParameter<UInt32>
    {
        public ManualWhiteBalanceParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ManualWhiteBalance, "White balance")
        {
        }

        public override void SetDefault()
        {
            Value = (UInt32)Minimum + (Maximum - Minimum) / 2;
        }
    }

    public class FlashPowerParameter : RangeParameter<UInt32>
    {
        public FlashPowerParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashPower, "Flash power")
        {
        }

        public override void SetDefault()
        {
            Value = (UInt32)Minimum + (Maximum - Minimum) / 2;
        }
    }
}