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
        private Guid _propertyId;
        private T _value;
        private T _minimum;
        private T _maximum;

        protected RangeParameter(PhotoCaptureDevice device, Guid propertyId, string name)
            : base(device, name)
        {
            _propertyId = propertyId;

            Refresh();
        }

        public override void Refresh()
        {
            try
            {
                CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, _propertyId);

                if (range == null)
                {
                    Supported = false;
                }
                else
                {
                    Minimum = (T)range.Min;
                    Maximum = (T)range.Max;
                    _value = (T)Device.GetProperty(_propertyId);
                    Supported = true;
                }
            }
            catch (Exception)
            {
                Supported = false;

                System.Diagnostics.Debug.WriteLine("Getting " + Name.ToLower() + " failed");
            }

            Modifiable = Supported && !_minimum.Equals(_maximum);

            if (Supported)
            {
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("OverlaySource");
            }
        }

        public T Minimum
        {
            get
            {
                return _minimum;
            }

            private set
            {
                if (!_minimum.Equals(value))
                {
                    _minimum = value;

                    NotifyPropertyChanged("Minimum");
                }
            }
        }

        public T Maximum
        {
            get
            {
                return _maximum;
            }

            private set
            {
                if (!_maximum.Equals(value))
                {
                    _maximum = value;

                    NotifyPropertyChanged("Maximum");
                }
            }
        }

        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (!_value.Equals(value))
                {
                    try
                    {
                        _value = value;

                        Device.SetProperty(_propertyId, (T)value);

                        NotifyPropertyChanged("Value");
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine("Setting " + Name.ToLower() + " failed");
                    }
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
            Value = (Int32)(Minimum + (Maximum - Minimum) / 2);
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
            Value = (UInt32)(Minimum + (Maximum - Minimum) / 2);
        }
    }
}