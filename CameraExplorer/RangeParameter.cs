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

            try
            {
                _range = PhotoCaptureDevice.GetSupportedPropertyRange(device.SensorLocation, guid);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Getting supported range for " + Name.ToLower() + " failed");
            }

            try
            {
                _value = (T)device.GetProperty(guid);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Getting " + Name.ToLower() + " failed");
            }
            
            try
            {
                if (_value == null)
                {
                    SetDefault();
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Setting " + Name.ToLower() + " failed");
            }

            Supported = _range != null;
            Modifiable = Supported && !_range.Min.Equals(_range.Max);
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

        protected override void SetDefault()
        {
            if (Minimum <= 0 && Maximum >= 0)
            {
                Device.SetProperty(KnownCameraPhotoProperties.ExposureCompensation, (Int32)0);
            }
            else
            {
                Device.SetProperty(KnownCameraPhotoProperties.ExposureCompensation, Minimum);
            }
        }
    }

    public class ManualWhiteBalanceParameter : RangeParameter<UInt32>
    {
        public ManualWhiteBalanceParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ManualWhiteBalance, "White balance")
        {
        }

        protected override void SetDefault()
        {
            Device.SetProperty(KnownCameraPhotoProperties.ManualWhiteBalance, Minimum);
        }
    }

    public class FlashPowerParameter : RangeParameter<UInt32>
    {
        public FlashPowerParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashPower, "Flash power")
        {
        }

        protected override void SetDefault()
        {
            Device.SetProperty(KnownCameraPhotoProperties.FlashPower, Minimum);
        }
    }
}