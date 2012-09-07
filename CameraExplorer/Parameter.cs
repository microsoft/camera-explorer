using Microsoft.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    public abstract class Parameter
    {
        protected PhotoCaptureDevice _device;
        string _name;

        protected Parameter(PhotoCaptureDevice device, string name)
        {
            _device = device;
            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        protected bool _supported = true;
        public bool Supported
        {
            get
            {
                return _supported;
            }
        }

        protected bool _modifiable = true;
        public bool Modifiable
        {
            get
            {
                return _modifiable;
            }
        }
    }

    public abstract class RangeParameter<T> : Parameter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Guid _guid;
        protected CameraCapturePropertyRange _range;

        protected RangeParameter(PhotoCaptureDevice device, Guid guid, string name)
            : base(device, name)
        {
            _device = device;
            _guid = guid;
            _range = PhotoCaptureDevice.GetSupportedPropertyRange(device.SensorLocation, guid);

            if (_device.GetProperty(guid) == null)
            {
                SetDefault();
            }

            _supported = _range != null;
            _modifiable = _supported && !_range.Min.Equals(_range.Max);
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

        public T Value
        {
            get
            {
                return (T)_device.GetProperty(_guid);
            }

            set
            {
                _device.SetProperty(_guid, (T)value);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        protected virtual void SetDefault()
        {
            throw new NotImplementedException();
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
            _device.SetProperty(KnownCameraPhotoProperties.FlashPower, Minimum);
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
            _device.SetProperty(KnownCameraPhotoProperties.ManualWhiteBalance, Minimum);
        }
    }

    public class ArrayParameterEnumerator<T> : IEnumerator<ArrayParameterItem<T>>
    {
        ArrayParameter<T> _arrayParameter;
        int _count;
        int _index = -1;

        public ArrayParameterEnumerator(ArrayParameter<T> arrayParameter, int count)
        {
            _arrayParameter = arrayParameter;
            _count = count;
        }

        public object Current
        {
            get
            {
                return _arrayParameter.Item(_index);
            }
        }

        ArrayParameterItem<T> IEnumerator<ArrayParameterItem<T>>.Current
        {
            get
            {
                return (ArrayParameterItem<T>)Current;
            }
        }

        public bool MoveNext()
        {
            if (_index < _count - 1)
            {
                _index++;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }
    }

    public class ArrayParameterItem<T>
    {
        T _value;
        string _name;

        public ArrayParameterItem(T value, string name)
        {
            _value = value;
            _name = name;
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }

    public abstract class ArrayParameter<T> : Parameter, IReadOnlyCollection<ArrayParameterItem<T>>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Guid _guid;
        List<ArrayParameterItem<T>> _items = new List<ArrayParameterItem<T>>();
        ArrayParameterItem<T> _selectedItem;

        public ArrayParameter(PhotoCaptureDevice device, string name)
            : base(device, name)
        {
            GetValues(ref _items, ref _selectedItem);
        }

        public ArrayParameter(PhotoCaptureDevice device, Guid guid, string name)
            : base(device, name)
        {
            _guid = guid;

            GetValues(ref _items, ref _selectedItem);

            _supported = _items.Count > 0;
            _modifiable = _supported && _items.Count > 1;
        }

        protected virtual void GetValues(ref List<ArrayParameterItem<T>> items, ref ArrayParameterItem<T> selectedItem)
        {
            object p = _device.GetProperty(_guid);

            if (p == null)
            {
                SetDefault();

                p = _device.GetProperty(_guid);
            }

            foreach (object o in PhotoCaptureDevice.GetSupportedPropertyValues(_device.SensorLocation, _guid))
            {
                dynamic dynamic_o = o;

                items.Add(CreateItemForValue((T)dynamic_o));

                if (o.Equals(p))
                {
                    selectedItem = items.Last();
                }
            }
        }

        protected virtual void SetValue(ArrayParameterItem<T> item)
        {
            _device.SetProperty(_guid, item.Value);
        }

        protected virtual void SetDefault()
        {
            throw new NotImplementedException();
        }

        public ArrayParameterItem<T> Item(int index)
        {
            return _items[index];
        }

        public virtual ArrayParameterItem<T> CreateItemForValue(T value)
        {
            return new ArrayParameterItem<T>(value, value.ToString());
        }

        public ArrayParameterItem<T> SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                if (value != null) // null check to avoid http://stackoverflow.com/questions/3446102
                {
                    _selectedItem = value;

                    SetValue(value);

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public IEnumerator<ArrayParameterItem<T>> GetEnumerator()
        {
            return new ArrayParameterEnumerator<T>(this, _items.Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayParameterEnumerator<T>(this, _items.Count);
        }
    }

    public class PreviewResolutionParameter : ArrayParameter<Windows.Foundation.Size>
    {
        public PreviewResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Preview resolution")
        {
        }

        protected override void GetValues(ref List<ArrayParameterItem<Windows.Foundation.Size>> items, ref ArrayParameterItem<Windows.Foundation.Size> selectedItem)
        {
            IReadOnlyList<Windows.Foundation.Size> list = PhotoCaptureDevice.GetAvailablePreviewResolutions(_device.SensorLocation);

            foreach (Windows.Foundation.Size s in list)
            {
                items.Add(new ArrayParameterItem<Windows.Foundation.Size>(s, s.Width + " x " + s.Height));

                if (_device.PreviewResolution == s)
                {
                    selectedItem = items.Last();
                }
            }
        }

        protected async override void SetValue(ArrayParameterItem<Windows.Foundation.Size> item)
        {
            await _device.SetPreviewResolutionAsync(item.Value);
        }
    }

    public class CaptureResolutionParameter : ArrayParameter<Windows.Foundation.Size>
    {
        public CaptureResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Capture resolution")
        {
        }

        protected override void GetValues(ref List<ArrayParameterItem<Windows.Foundation.Size>> items, ref ArrayParameterItem<Windows.Foundation.Size> selectedItem)
        {
            IReadOnlyList<Windows.Foundation.Size> list = PhotoCaptureDevice.GetAvailableCaptureResolutions(_device.SensorLocation);

            foreach (Windows.Foundation.Size s in list)
            {
                items.Add(new ArrayParameterItem<Windows.Foundation.Size>(s, s.Width + " x " + s.Height));

                if (_device.CaptureResolution == s)
                {
                    selectedItem = items.Last();
                }
            }
        }

        protected async override void SetValue(ArrayParameterItem<Windows.Foundation.Size> item)
        {
            await _device.SetCaptureResolutionAsync(item.Value);
        }
    }

    public class ExposureTimeParameter : ArrayParameter<UInt32>
    {
        public ExposureTimeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ExposureTime, "Exposure time")
        {
        }

        List<Tuple<UInt32, string>> ConvertRangeToArray(UInt32 min, UInt32 max)
        {
            List<Tuple<UInt32, string>> list = new List<Tuple<UInt32, string>>();

            list.Add(new Tuple<UInt32, string>(1000000 / 16000, "1/16000 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 8000, "1/8000 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 4000, "1/4000 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 2000, "1/2000 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 1000, "1/1000 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 500, "1/500 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 250, "1/250 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 125, "1/125 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 60, "1/60 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 30, "1/30 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 15, "1/15 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 8, "1/8 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 4, "1/4 s"));
            list.Add(new Tuple<UInt32, string>(1000000 / 2, "1/2 s"));
            list.Add(new Tuple<UInt32, string>(1000000, "1 s"));

            for (int i = 0; i < list.Count;)
            {
                Tuple<UInt32, string> o = list[i];

                if (o.Item1 < min || o.Item1 > max)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return list;
        }

        protected override void GetValues(ref List<ArrayParameterItem<UInt32>> items, ref ArrayParameterItem<UInt32> selectedItem)
        {
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(_device.SensorLocation, _guid);

            object p = _device.GetProperty(_guid);

            List<Tuple<UInt32, string>> array = ConvertRangeToArray((UInt32)range.Min, (UInt32)range.Max);

            if (p == null)
            {
                p = array[0].Item1;

                _device.SetProperty(_guid, (UInt32)p);
            }

            foreach (Tuple<UInt32, string> o in array)
            {
                items.Add(new ArrayParameterItem<UInt32>(o.Item1, o.Item2));

                if (o.Item1.Equals(p))
                {
                    selectedItem = items.Last();
                }
            }
        }
    }

    public class ExposureCompensationParameter : ArrayParameter<Int32>
    {
        public ExposureCompensationParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ExposureCompensation, "Exposure compensation")
        {
        }

        IReadOnlyList<Int32> ConvertRangeToArray(Int32 min, Int32 max)
        {
            List<Int32> list = new List<Int32>();

            for (; min <= max; min++)
            {
                list.Add(min);
            }

            return list;
        }

        protected override void GetValues(ref List<ArrayParameterItem<Int32>> items, ref ArrayParameterItem<Int32> selectedItem)
        {
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(_device.SensorLocation, _guid);

            object p = _device.GetProperty(_guid);

            IReadOnlyList<Int32> array = ConvertRangeToArray((Int32)range.Min, (Int32)range.Max);

            if (p == null)
            {
                p = array[0];

                _device.SetProperty(_guid, (Int32)p);
            }

            foreach (Int32 o in array)
            {
                items.Add(CreateItemForValue(o));

                if (o.Equals(p))
                {
                    selectedItem = items.Last();
                }
            }
        }
    }

    public class IsoParameter : ArrayParameter<UInt32>
    {
        public IsoParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.Iso, "ISO")
        {
        }

        IReadOnlyList<UInt32> ConvertRangeToArray(UInt32 min, UInt32 max)
        {
            List<UInt32> list = new List<UInt32>();

            min = min > 100 ? min : 100;

            while (min <= max)
            {
                list.Add(min);
                min *= 2;
            }

            return list;
        }

        protected override void GetValues(ref List<ArrayParameterItem<UInt32>> items, ref ArrayParameterItem<UInt32> selectedItem)
        {
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(_device.SensorLocation, _guid);

            object p = _device.GetProperty(_guid);

            IReadOnlyList<UInt32> array = ConvertRangeToArray((UInt32)range.Min, (UInt32)range.Max);

            if (p == null)
            {
                p = array[0];

                _device.SetProperty(_guid, (UInt32)p);
            }

            foreach (UInt32 o in array)
            {
                items.Add(CreateItemForValue(o));

                if (o.Equals(p))
                {
                    selectedItem = items.Last();
                }
            }
        }
    }

    public class SceneModeParameter : ArrayParameter<CameraSceneMode> // todo CameraSceneMode
    {
        public SceneModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.SceneMode, "Scene mode")
        {
        }

        public override ArrayParameterItem<CameraSceneMode> CreateItemForValue(CameraSceneMode value)
        {
            string name;

            switch (value)
            {
                case CameraSceneMode.Auto:
                    name = "Auto";
                    break;
                case CameraSceneMode.Portrait:
                    name = "Portrait";
                    break;
                case CameraSceneMode.Sport:
                    name = "Sport";
                    break;
                case CameraSceneMode.Snow:
                    name = "Snow";
                    break;
                case CameraSceneMode.Night:
                    name = "Night";
                    break;
                case CameraSceneMode.Beach:
                    name = "Beach";
                    break;
                case CameraSceneMode.Sunset:
                    name = "Sunset";
                    break;
                case CameraSceneMode.Candlelight:
                    name = "Candlelight";
                    break;
                case CameraSceneMode.Landscape:
                    name = "Landscape";
                    break;
                case CameraSceneMode.NightPortrait:
                    name = "NightPortrait";
                    break;
                case CameraSceneMode.Backlit:
                    name = "Backlit";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return new ArrayParameterItem<CameraSceneMode>(value, name);
        }

        protected override void SetDefault()
        {
            _device.SetProperty(KnownCameraPhotoProperties.SceneMode, CameraSceneMode.Auto);
        }
    }

    public class FlashModeParameter : ArrayParameter<FlashMode>
    {
        public FlashModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashMode, "Flash mode")
        {
        }

        public override ArrayParameterItem<FlashMode> CreateItemForValue(FlashMode value)
        {
            string name;

            switch (value)
            {
                case FlashMode.Auto:
                    name = "Auto";
                    break;
                case FlashMode.Off:
                    name = "Off";
                    break;
                case FlashMode.On:
                    name = "On";
                    break;
                case FlashMode.RedEyeReduction:
                    name = "RedEyeReduction";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return new ArrayParameterItem<FlashMode>(value, name);
        }

        protected override void SetDefault()
        {
            _device.SetProperty(KnownCameraPhotoProperties.FlashMode, FlashMode.Auto);
        }
    }

    public class FocusIlluminationModeParameter : ArrayParameter<FocusIlluminationMode>
    {
        public FocusIlluminationModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FocusIlluminationMode, "Focus illumination mode")
        {
        }

        public override ArrayParameterItem<FocusIlluminationMode> CreateItemForValue(FocusIlluminationMode value)
        {
            string name;

            switch (value)
            {
                case FocusIlluminationMode.Auto:
                    name = "Auto";
                    break;
                case FocusIlluminationMode.Off:
                    name = "Off";
                    break;
                case FocusIlluminationMode.On:
                    name = "On";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return new ArrayParameterItem<FocusIlluminationMode>(value, name);
        }

        protected override void SetDefault()
        {
            _device.SetProperty(KnownCameraPhotoProperties.FocusIlluminationMode, FocusIlluminationMode.Auto);
        }
    }

    public class WhiteBalancePresetParameter : ArrayParameter<WhiteBalancePreset>
    {
        public WhiteBalancePresetParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.WhiteBalancePreset, "White balance preset")
        {
        }

        public override ArrayParameterItem<WhiteBalancePreset> CreateItemForValue(WhiteBalancePreset value)
        {
            string name;

            switch (value)
            {
                case WhiteBalancePreset.Candlelight:
                    name = "Candlelight";
                    break;
                case WhiteBalancePreset.Cloudy:
                    name = "Cloudy";
                    break;
                case WhiteBalancePreset.Daylight:
                    name = "Daylight";
                    break;
                case WhiteBalancePreset.Flash:
                    name = "Flash";
                    break;
                case WhiteBalancePreset.Fluorescent:
                    name = "Fluorescent";
                    break;
                case WhiteBalancePreset.Tungsten:
                    name = "Tungsten";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return new ArrayParameterItem<WhiteBalancePreset>(value, name);
        }

        protected override void SetDefault()
        {
        }
    }

    public class AutoFocusRangeParameter : ArrayParameter<AutoFocusRange>
    {
        public AutoFocusRangeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraGeneralProperties.AutoFocusRange, "Auto focus range")
        {
        }

        public override ArrayParameterItem<AutoFocusRange> CreateItemForValue(AutoFocusRange value)
        {
            string name;

            switch (value)
            {
                case AutoFocusRange.Full:
                    name = "Full";
                    break;
                case AutoFocusRange.Hyperfocal:
                    name = "Hyperfocal";
                    break;
                case AutoFocusRange.Infinity:
                    name = "Infinity";
                    break;
                case AutoFocusRange.Macro:
                    name = "Macro";
                    break;
                case AutoFocusRange.Normal:
                    name = "Normal";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return new ArrayParameterItem<AutoFocusRange>(value, name);
        }

        protected override void SetDefault()
        {
        }
    }
}