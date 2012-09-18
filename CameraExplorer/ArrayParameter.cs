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
        public ArrayParameterItem(T value, string name, string imageSource = null)
        {
            _value = value;
            _name = name;
            _imageSource = imageSource;
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

        protected string _imageSource;
        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
        }
    }

    public abstract class ArrayParameter<T> : Parameter, IReadOnlyCollection<ArrayParameterItem<T>>
    {
        List<ArrayParameterItem<T>> _items = new List<ArrayParameterItem<T>>();
        ArrayParameterItem<T> _selectedItem;

        Guid _guid;
        protected Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        public ArrayParameter(PhotoCaptureDevice device, string name, bool overlay = false)
            : base(device, name, overlay)
        {
            Refresh();
        }

        public ArrayParameter(PhotoCaptureDevice device, Guid guid, string name, bool overlay = false)
            : base(device, name, overlay)
        {
            _guid = guid;

            Refresh();
        }

        public override void Refresh()
        {
            _items.Clear();

            _selectedItem = null;

            bool supported = false;

            try
            {
                GetValues(ref _items, ref _selectedItem);

                supported = _items.Count > 0;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Getting " + Name.ToLower() + " failed");
            }

            Supported = supported;
            Modifiable = Supported && _items.Count > 1;

            if (supported)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("SelectedItem");
                NotifyPropertyChanged("ImageSource");
            }
        }

        protected virtual void GetValues(ref List<ArrayParameterItem<T>> items, ref ArrayParameterItem<T> selectedItem)
        {
            object p = Device.GetProperty(_guid);

            foreach (object o in PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, _guid))
            {
                dynamic dynamic_o = o;

                ArrayParameterItem<T> item = CreateItemForValue((T)dynamic_o);

                if (item != null)
                {
                    items.Add(item);

                    if (o.Equals(p))
                    {
                        selectedItem = items.Last();

                        ImageSource = selectedItem.ImageSource;
                    }
                }
            }
        }

        protected virtual void SetValue(ArrayParameterItem<T> item)
        {
            Device.SetProperty(_guid, (T)item.Value);
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
                    try
                    {
                        SetValue(value);

                        _selectedItem = value;

                        ImageSource = _selectedItem.ImageSource;
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine("Setting " + Name.ToLower() + " failed");
                    }
                }

                NotifyPropertyChanged("SelectedItem");
                NotifyPropertyChanged("ImageSource");
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
        ArrayParameterItem<Windows.Foundation.Size> _defaultItem;

        public PreviewResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Preview resolution")
        {
        }

        protected override void GetValues(ref List<ArrayParameterItem<Windows.Foundation.Size>> items, ref ArrayParameterItem<Windows.Foundation.Size> selectedItem)
        {
            IReadOnlyList<Windows.Foundation.Size> list = PhotoCaptureDevice.GetAvailablePreviewResolutions(Device.SensorLocation);

            foreach (Windows.Foundation.Size s in list)
            {
                items.Add(new ArrayParameterItem<Windows.Foundation.Size>(s, s.Width + " x " + s.Height));

                if (Device.PreviewResolution == s)
                {
                    selectedItem = items.Last();
                }
            }

            if (items.Count > 0)
            {
                _defaultItem = items.First();
            }
            else
            {
                _defaultItem = null;
            }
        }

        protected async override void SetValue(ArrayParameterItem<Windows.Foundation.Size> item)
        {
            Modifiable = false;

            await Device.SetPreviewResolutionAsync(item.Value);

            Modifiable = true;
        }
    }

    public class CaptureResolutionParameter : ArrayParameter<Windows.Foundation.Size>
    {
        ArrayParameterItem<Windows.Foundation.Size> _defaultItem;

        public CaptureResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Capture resolution")
        {
        }

        protected override void GetValues(ref List<ArrayParameterItem<Windows.Foundation.Size>> items, ref ArrayParameterItem<Windows.Foundation.Size> selectedItem)
        {
            IReadOnlyList<Windows.Foundation.Size> list = PhotoCaptureDevice.GetAvailableCaptureResolutions(Device.SensorLocation);

            foreach (Windows.Foundation.Size s in list)
            {
                items.Add(new ArrayParameterItem<Windows.Foundation.Size>(s, s.Width + " x " + s.Height));

                if (Device.CaptureResolution == s)
                {
                    selectedItem = items.Last();
                }
            }

            if (items.Count > 0)
            {
                _defaultItem = items.First();
            }
            else
            {
                _defaultItem = null;
            }
        }

        protected async override void SetValue(ArrayParameterItem<Windows.Foundation.Size> item)
        {
            Modifiable = false;

            await Device.SetCaptureResolutionAsync(item.Value);

            Modifiable = true;
        }
    }

    public class ExposureTimeParameter : ArrayParameter<UInt32>
    {
        ArrayParameterItem<UInt32> _defaultItem;

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
                UInt32 o = 1000000 / list[i];

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
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, Guid);

            object p = Device.GetProperty(Guid);

            List<UInt32> array = ConvertRangeToArray((UInt32)range.Min, (UInt32)range.Max);

            foreach (UInt32 o in array)
            {
                ArrayParameterItem<UInt32> item = new ArrayParameterItem<UInt32>(
                    1000000 / o,
                    "1/" + o.ToString() + " s",
                    "Assets/Icons/overlay.exposuretime." + o.ToString() + ".png");

                if (item != null)
                {
                    items.Add(item);

                    if ((1000000 / o).Equals(p))
                    {
                        selectedItem = items.Last();

                        ImageSource = selectedItem.ImageSource;
                    }
                }
            }

            if (items.Count > 0)
            {
                _defaultItem = items.Last();
            }
            else
            {
                _defaultItem = null;
            }
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class IsoParameter : ArrayParameter<UInt32>
    {
        ArrayParameterItem<UInt32> _defaultItem;

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
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, Guid);

            object p = Device.GetProperty(Guid);

            IReadOnlyList<UInt32> array = ConvertRangeToArray((UInt32)range.Min, (UInt32)range.Max);

            foreach (UInt32 o in array)
            {
                ArrayParameterItem<UInt32> item = CreateItemForValue(o);

                if (item != null)
                {
                    items.Add(item);

                    if (o.Equals(p))
                    {
                        selectedItem = items.Last();

                        ImageSource = selectedItem.ImageSource;
                    }
                }
            }

            if (items.Count > 0)
            {
                _defaultItem = items[0];
            }
            else
            {
                _defaultItem = null;
            }
        }

        public override ArrayParameterItem<UInt32> CreateItemForValue(uint value)
        {
            string name = "ISO " + value.ToString();
            string imageSource = "Assets/Icons/overlay.iso." + value.ToString() + ".png";

            return new ArrayParameterItem<UInt32>(value, name, imageSource);
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class SceneModeParameter : ArrayParameter<CameraSceneMode>
    {
        ArrayParameterItem<CameraSceneMode> _defaultItem;

        public SceneModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.SceneMode, "Scene mode", true)
        {
        }

        public override ArrayParameterItem<CameraSceneMode> CreateItemForValue(CameraSceneMode value)
        {
            string name;
            string imageSource;

            switch (value)
            {
                case CameraSceneMode.Auto:
                    name = "Auto";
                    imageSource = "Assets/Icons/overlay.scenemode.auto.png";
                    break;
                case CameraSceneMode.Portrait:
                    name = "Portrait";
                    imageSource = "Assets/Icons/overlay.scenemode.portrait.png";
                    break;
                case CameraSceneMode.Sport:
                    name = "Sport";
                    imageSource = "Assets/Icons/overlay.scenemode.sport.png";
                    break;
                case CameraSceneMode.Snow:
                    name = "Snow";
                    imageSource = "Assets/Icons/overlay.scenemode.snow.png";
                    break;
                case CameraSceneMode.Night:
                    name = "Night";
                    imageSource = "Assets/Icons/overlay.scenemode.night.png";
                    break;
                case CameraSceneMode.Beach:
                    name = "Beach";
                    imageSource = "Assets/Icons/overlay.scenemode.beach.png";
                    break;
                case CameraSceneMode.Sunset:
                    name = "Sunset";
                    imageSource = "Assets/Icons/overlay.scenemode.sunset.png";
                    break;
                case CameraSceneMode.Candlelight:
                    name = "Candlelight";
                    imageSource = "Assets/Icons/overlay.scenemode.candlelight.png";
                    break;
                case CameraSceneMode.Landscape:
                    name = "Landscape";
                    imageSource = "Assets/Icons/overlay.scenemode.landscape.png";
                    break;
                case CameraSceneMode.NightPortrait:
                    name = "Night portrait";
                    imageSource = "Assets/Icons/overlay.scenemode.nightportrait.png";
                    break;
                case CameraSceneMode.Backlit:
                    name = "Backlit";
                    imageSource = "Assets/Icons/overlay.scenemode.backlit.png";
                    break;
                default:
                    name = null;
                    imageSource = null;
                    break;
            }

            ArrayParameterItem<CameraSceneMode> item = null;

            if (name != null)
            {
                item = new ArrayParameterItem<CameraSceneMode>(value, name, imageSource);

                if (_defaultItem == null)
                {
                    _defaultItem = item;
                }
            }

            return item;
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class FlashModeParameter : ArrayParameter<FlashMode>
    {
        ArrayParameterItem<FlashMode> _defaultItem;

        public FlashModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashMode, "Flash mode", true)
        {
        }

        public override ArrayParameterItem<FlashMode> CreateItemForValue(FlashMode value)
        {
            string name;
            string imageSource;

            switch (value)
            {
                case FlashMode.Auto:
                    name = "Auto";
                    imageSource = "Assets/Icons/overlay.flashmode.auto.png";
                    break;
                case FlashMode.Off:
                    name = "Off";
                    imageSource = "Assets/Icons/overlay.flashmode.off.png";
                    break;
                case FlashMode.On:
                    name = "On";
                    imageSource = "Assets/Icons/overlay.flashmode.on.png";
                    break;
                case FlashMode.RedEyeReduction:
                    name = "Red-eye reduction";
                    imageSource = "Assets/Icons/overlay.flashmode.redeyereduction.png";
                    break;
                default:
                    name = null;
                    imageSource = null;
                    break;
            }

            ArrayParameterItem<FlashMode> item = null;

            if (name != null)
            {
                item = new ArrayParameterItem<FlashMode>(value, name, imageSource);

                if (_defaultItem == null)
                {
                    _defaultItem = item;
                }
            }

            return item;
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class FocusIlluminationModeParameter : ArrayParameter<FocusIlluminationMode>
    {
        ArrayParameterItem<FocusIlluminationMode> _defaultItem;

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
                    name = null;
                    break;
            }

            ArrayParameterItem<FocusIlluminationMode> item = null;

            if (name != null)
            {
                item = new ArrayParameterItem<FocusIlluminationMode>(value, name);

                if (_defaultItem == null)
                {
                    _defaultItem = item;
                }
            }

            return item;
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class WhiteBalancePresetParameter : ArrayParameter<WhiteBalancePreset>
    {
        ArrayParameterItem<WhiteBalancePreset> _defaultItem;

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
                    name = null;
                    break;
            }

            ArrayParameterItem<WhiteBalancePreset> item = null;

            if (name != null)
            {
                item = new ArrayParameterItem<WhiteBalancePreset>(value, name);

                if (_defaultItem == null)
                {
                    _defaultItem = item;
                }
            }

            return item;
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }

    public class AutoFocusRangeParameter : ArrayParameter<AutoFocusRange>
    {
        ArrayParameterItem<AutoFocusRange> _defaultItem;

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
                    name = null;
                    break;
            }

            ArrayParameterItem<AutoFocusRange> item = null;

            if (name != null)
            {
                item = new ArrayParameterItem<AutoFocusRange>(value, name);

                if (_defaultItem == null)
                {
                    _defaultItem = item;
                }
            }

            return item;
        }

        public override void SetDefault()
        {
            SelectedItem = _defaultItem;
        }
    }
}