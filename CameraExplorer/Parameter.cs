using Microsoft.Devices;
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
    public abstract class Parameter
    {
        protected PhotoCaptureDevice _device;

        protected Parameter(PhotoCaptureDevice device, string name, bool overlay)
        {
            _device = device;
            _name = name;

            if (overlay)
            {
                _image = "";
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        protected string _image;
        public string Image
        {
            get
            {
                return _image;
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

        protected virtual void SetDefault()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class RangeParameter<T> : Parameter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Guid _guid;
        protected CameraCapturePropertyRange _range;

        protected RangeParameter(PhotoCaptureDevice device, Guid guid, string name, bool overlay = false)
            : base(device, name, overlay)
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
        public ArrayParameterItem(T value, string name, string image = null)
        {
            _value = value;
            _name = name;
            _image = image;
        }

        T _value;
        public T Value
        {
            get
            {
                return _value;
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        protected string _image;
        public string Image
        {
            get
            {
                return _image;
            }
        }
    }

    public abstract class ArrayParameter<T> : Parameter, IReadOnlyCollection<ArrayParameterItem<T>>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Guid _guid;
        List<ArrayParameterItem<T>> _items = new List<ArrayParameterItem<T>>();
        ArrayParameterItem<T> _selectedItem;

        public ArrayParameter(PhotoCaptureDevice device, string name, bool overlay = false)
            : base(device, name, overlay)
        {
            GetValues(ref _items, ref _selectedItem);
        }

        public ArrayParameter(PhotoCaptureDevice device, Guid guid, string name, bool overlay = false)
            : base(device, name, overlay)
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
                    _image = selectedItem.Image;
                }
            }
        }

        protected virtual void SetValue(ArrayParameterItem<T> item)
        {
            _device.SetProperty(_guid, (T)item.Value);
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

                    _image = value.Image;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                        PropertyChanged(this, new PropertyChangedEventArgs("Image"));
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
            : base(device, KnownCameraPhotoProperties.ExposureTime, "Exposure time", true)
        {
        }

        List<UInt32> ConvertRangeToArray(UInt32 min, UInt32 max)
        {
            List<UInt32> list = new List<UInt32>();

            list.Add(16000);
            list.Add(8000);
            list.Add(4000);
            list.Add(2000);
            list.Add(1000);
            list.Add(500);
            list.Add(250);
            list.Add(125);
            list.Add(60);
            list.Add(30);
            list.Add(15);
            list.Add(8);
            list.Add(4);
            list.Add(2);
            list.Add(1);

            for (int i = 0; i < list.Count;)
            {
                UInt32 o = list[i];

                if (o < min || o > max)
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

            List<UInt32> array = ConvertRangeToArray((UInt32)range.Min, (UInt32)range.Max);

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
                    _image = selectedItem.Image;
                }
            }
        }

        public override ArrayParameterItem<UInt32> CreateItemForValue(uint value)
        {
            string name = "1/" + value.ToString() + " s";
            string image = "Assets/Icons/overlay.exposuretime." + value.ToString() + ".png";

            return new ArrayParameterItem<UInt32>(value, name, image);
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
            if ((Int32)_range.Min <= 0 && (Int32)_range.Max >= 0)
            {
                _device.SetProperty(KnownCameraPhotoProperties.ExposureCompensation, (Int32)0);
            }
            else
            {
                _device.SetProperty(KnownCameraPhotoProperties.ExposureCompensation, (Int32)_range.Min);
            }
        }
    }

    public class IsoParameter : ArrayParameter<UInt32>
    {
        public IsoParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.Iso, "ISO", true)
        {
        }

        IReadOnlyList<UInt32> ConvertRangeToArray(UInt32 min, UInt32 max)
        {
            List<UInt32> list = new List<UInt32>();

            list.Add(100);
            list.Add(200);
            list.Add(400);
            list.Add(800);
            list.Add(1600);
            list.Add(3200);

            for (int i = 0; i < list.Count; )
            {
                UInt32 o = list[i];

                if (o < min || o > max)
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
                    _image = selectedItem.Image;
                }
            }
        }

        public override ArrayParameterItem<UInt32> CreateItemForValue(uint value)
        {
            string name = "ISO " + value.ToString();
            string image = "Assets/Icons/overlay.iso." + value.ToString() + ".png";

            return new ArrayParameterItem<UInt32>(value, name, image);
        }
    }

    public class SceneModeParameter : ArrayParameter<CameraSceneMode> // todo CameraSceneMode
    {
        public SceneModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.SceneMode, "Scene mode", true)
        {
        }

        public override ArrayParameterItem<CameraSceneMode> CreateItemForValue(CameraSceneMode value)
        {
            string name;
            string image;

            switch (value)
            {
                case CameraSceneMode.Auto:
                    name = "Auto";
                    image = "Assets/Icons/overlay.scenemode.auto.png";
                    break;
                case CameraSceneMode.Portrait:
                    name = "Portrait";
                    image = "Assets/Icons/overlay.scenemode.portrait.png";
                    break;
                case CameraSceneMode.Sport:
                    name = "Sport";
                    image = "Assets/Icons/overlay.scenemode.sport.png";
                    break;
                case CameraSceneMode.Snow:
                    name = "Snow";
                    image = "Assets/Icons/overlay.scenemode.snow.png";
                    break;
                case CameraSceneMode.Night:
                    name = "Night";
                    image = "Assets/Icons/overlay.scenemode.night.png";
                    break;
                case CameraSceneMode.Beach:
                    name = "Beach";
                    image = "Assets/Icons/overlay.scenemode.beach.png";
                    break;
                case CameraSceneMode.Sunset:
                    name = "Sunset";
                    image = "Assets/Icons/overlay.scenemode.sunset.png";
                    break;
                case CameraSceneMode.Candlelight:
                    name = "Candlelight";
                    image = "Assets/Icons/overlay.scenemode.candlelight.png";
                    break;
                case CameraSceneMode.Landscape:
                    name = "Landscape";
                    image = "Assets/Icons/overlay.scenemode.landscape.png";
                    break;
                case CameraSceneMode.NightPortrait:
                    name = "Night portrait";
                    image = "Assets/Icons/overlay.scenemode.nightportrait.png";
                    break;
                case CameraSceneMode.Backlit:
                    name = "Backlit";
                    image = "Assets/Icons/overlay.scenemode.backlit.png";
                    break;
                default:
                    name = "Unknown";
                    image = "";
                    break;
            }

            return new ArrayParameterItem<CameraSceneMode>(value, name, image);
        }

        protected override void SetDefault()
        {
            _device.SetProperty(KnownCameraPhotoProperties.SceneMode, CameraSceneMode.Auto);
        }
    }

    public class FlashModeParameter : ArrayParameter<FlashMode>
    {
        public FlashModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashMode, "Flash mode", true)
        {
        }

        public override ArrayParameterItem<FlashMode> CreateItemForValue(FlashMode value)
        {
            string name;
            string image;

            switch (value)
            {
                case FlashMode.Auto:
                    name = "Auto";
                    image = "Assets/Icons/overlay.flashmode.auto.png";
                    break;
                case FlashMode.Off:
                    name = "Off";
                    image = "Assets/Icons/overlay.flashmode.off.png";
                    break;
                case FlashMode.On:
                    name = "On";
                    image = "Assets/Icons/overlay.flashmode.on.png";
                    break;
                case FlashMode.RedEyeReduction:
                    name = "Red-eye reduction";
                    image = "Assets/Icons/overlay.flashmode.redeyereduction.png";
                    break;
                default:
                    name = "Unknown";
                    image = "";
                    break;
            }

            return new ArrayParameterItem<FlashMode>(value, name, image);
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