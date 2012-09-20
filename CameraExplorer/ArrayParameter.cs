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
    public class ArrayParameterEnumerator<T> : IEnumerator<ArrayParameterOption<T>>
    {
        private ArrayParameter<T> _arrayParameter;
        private int _count;
        private int _index = -1;

        public ArrayParameterEnumerator(ArrayParameter<T> arrayParameter, int count)
        {
            _arrayParameter = arrayParameter;
            _count = count;
        }

        public object Current
        {
            get
            {
                return _arrayParameter.Option(_index);
            }
        }

        ArrayParameterOption<T> IEnumerator<ArrayParameterOption<T>>.Current
        {
            get
            {
                return (ArrayParameterOption<T>)Current;
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

    public class ArrayParameterOption<T>
    {
        private T _value;
        private string _name;
        private string _overlaySource;

        public ArrayParameterOption(T value, string name, string overlaySource = null)
        {
            _value = value;
            _name = name;
            _overlaySource = overlaySource;
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

        public string OverlaySource
        {
            get
            {
                return _overlaySource;
            }
        }
    }

    public abstract class ArrayParameter<T> : Parameter, IReadOnlyCollection<ArrayParameterOption<T>>
    {
        private List<ArrayParameterOption<T>> _options = new List<ArrayParameterOption<T>>();
        private ArrayParameterOption<T> _selectedOption;
        private Guid _guid;

        public ArrayParameter(PhotoCaptureDevice device, string name)
            : base(device, name)
        {
        }

        public ArrayParameter(PhotoCaptureDevice device, Guid guid, string name)
            : base(device, name)
        {
            _guid = guid;
        }

        public override void Refresh()
        {
            _options.Clear();

            try
            {
                PopulateOptions();

                Supported = _options.Count > 0;
            }
            catch (Exception)
            {
                Supported = false;

                System.Diagnostics.Debug.WriteLine("Getting " + Name.ToLower() + " failed");
            }

            Modifiable = Supported && _options.Count > 1;

            if (Supported)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("SelectedOption");
                NotifyPropertyChanged("OverlaySource");
            }
        }

        public ArrayParameterOption<T> Option(int index)
        {
            return _options[index];
        }

        public ArrayParameterOption<T> SelectedOption
        {
            get
            {
                return _selectedOption;
            }

            set
            {
                if (value == null) return; // null check to avoid http://stackoverflow.com/questions/3446102

                if (_selectedOption != value)
                {
                    SetOption(value);

                    _selectedOption = value;

                    OverlaySource = _selectedOption.OverlaySource;

                    NotifyPropertyChanged("SelectedOption");
                    NotifyPropertyChanged("OverlaySource");
                }
            }
        }

        public int Count
        {
            get
            {
                return _options.Count;
            }
        }

        public IEnumerator<ArrayParameterOption<T>> GetEnumerator()
        {
            return new ArrayParameterEnumerator<T>(this, _options.Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayParameterEnumerator<T>(this, _options.Count);
        }

        protected Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        protected List<ArrayParameterOption<T>> Options
        {
            get
            {
                return _options;
            }
        }

        protected virtual void PopulateOptions()
        {
            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, _guid);
            object value = Device.GetProperty(_guid);

            foreach (dynamic i in supportedValues)
            {
                ArrayParameterOption<T> item = CreateOption((T)i);

                Options.Add(item);

                if (i.Equals(value))
                {
                    SelectedOption = item;
                    OverlaySource = item.OverlaySource;
                }
            }
        }

        protected abstract ArrayParameterOption<T> CreateOption(T value);

        protected virtual void SetOption(ArrayParameterOption<T> item)
        {
            Device.SetProperty(_guid, (T)item.Value);
        }
    }

    public class PreviewResolutionParameter : ArrayParameter<Windows.Foundation.Size>
    {
        public PreviewResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Preview resolution")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<Windows.Foundation.Size> supportedValues = PhotoCaptureDevice.GetAvailablePreviewResolutions(Device.SensorLocation);
            Windows.Foundation.Size value = Device.PreviewResolution;

            ArrayParameterOption<Windows.Foundation.Size> item = null;

            foreach (Windows.Foundation.Size i in supportedValues)
            {
                item = CreateOption(i);

                Options.Add(item);

                if (i.Equals(value))
                {
                    SelectedOption = item;
                }
            }
        }

        protected override ArrayParameterOption<Windows.Foundation.Size> CreateOption(Windows.Foundation.Size value)
        {
            string name = value.Width + " x " + value.Height;

            return new ArrayParameterOption<Windows.Foundation.Size>(value, name);
        }

        protected async override void SetOption(ArrayParameterOption<Windows.Foundation.Size> item)
        {
            Modifiable = false;

            await Device.SetPreviewResolutionAsync(item.Value);

            Modifiable = true;
        }

        public override void SetDefault()
        {
            if (Options.Count > 0)
            {
                SetOption(Options.First());
            }
            else
            {
                SelectedOption = null;
            }
        }
    }

    public class CaptureResolutionParameter : ArrayParameter<Windows.Foundation.Size>
    {
        public CaptureResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Capture resolution")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<Windows.Foundation.Size> supportedValues = PhotoCaptureDevice.GetAvailableCaptureResolutions(Device.SensorLocation);
            Windows.Foundation.Size value = Device.CaptureResolution;

            ArrayParameterOption<Windows.Foundation.Size> item = null;

            foreach (Windows.Foundation.Size i in supportedValues)
            {
                item = CreateOption(i);

                Options.Add(item);

                if (i.Equals(value))
                {
                    SelectedOption = item;
                }
            }
        }

        protected override ArrayParameterOption<Windows.Foundation.Size> CreateOption(Windows.Foundation.Size value)
        {
            string name = value.Width + " x " + value.Height;

            return new ArrayParameterOption<Windows.Foundation.Size>(value, name);
        }

        protected async override void SetOption(ArrayParameterOption<Windows.Foundation.Size> item)
        {
            Modifiable = false;

            await Device.SetCaptureResolutionAsync(item.Value);

            Modifiable = true;
        }

        public override void SetDefault()
        {
            if (Options.Count > 0)
            {
                SetOption(Options.First());
            }
            else
            {
                SelectedOption = null;
            }
        }
    }

    public class ExposureTimeParameter : ArrayParameter<UInt32>
    {
        private ArrayParameterOption<UInt32> _defaultOption;

        public ExposureTimeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ExposureTime, "Exposure time")
        {
        }

        protected override void PopulateOptions()
        {
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, Guid);
            object value = Device.GetProperty(Guid);
            UInt32[] standardValues = { 16000, 8000, 4000, 2000, 1000, 500, 250, 125, 60, 30, 15, 8, 4, 2, 1 };

            UInt32 min = (UInt32)range.Min;
            UInt32 max = (UInt32)range.Max;

            ArrayParameterOption<UInt32> item = null;

            foreach (UInt32 i in standardValues)
            {
                UInt32 usecs = 1000000 / i;

                if (usecs >= min && usecs <= max)
                {
                    item = CreateOption(i);

                    Options.Add(item);

                    if (usecs.Equals(value))
                    {
                        SelectedOption = item;
                        OverlaySource = item.OverlaySource;
                    }

                    if (i >= 30)
                    {
                        _defaultOption = item;
                    }
                }
            }
        }

        protected override ArrayParameterOption<UInt32> CreateOption(UInt32 value)
        {
            string name = "1 / " + value.ToString() + " s";
            string overlaySource = "Assets/Icons/overlay.exposuretime." + value.ToString() + ".png";

            return new ArrayParameterOption<UInt32>(1000000 / value, name, overlaySource);
        }

        public override void SetDefault()
        {
            if (Options.Count > 0)
            {
                foreach (ArrayParameterOption<UInt32> i in Options)
                {
                    if (i.Value == 1000000 / 30 || i == Options.Last())
                    {
                        SelectedOption = i;
                        break;
                    }
                }
            }
            else
            {
                SelectedOption = null;
            }
        }
    }

    public class IsoParameter : ArrayParameter<UInt32>
    {
        public IsoParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.Iso, "ISO")
        {
        }

        protected override void PopulateOptions()
        {
            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, Guid);
            object value = Device.GetProperty(Guid);
            UInt32[] standardValues = { 100, 200, 400, 800, 1600, 3200 };

            UInt32 min = (UInt32)range.Min;
            UInt32 max = (UInt32)range.Max;

            ArrayParameterOption<UInt32> item = null;

            foreach (UInt32 i in standardValues)
            {
                if (i >= min && i <= max)
                {
                    item = CreateOption(i);

                    Options.Add(item);

                    if (i.Equals(value))
                    {
                        SelectedOption = item;
                        OverlaySource = item.OverlaySource;
                    }
                }
            }
        }

        protected override ArrayParameterOption<UInt32> CreateOption(UInt32 value)
        {
            string name = "ISO " + value.ToString();
            string overlaySource = "Assets/Icons/overlay.iso." + value.ToString() + ".png";

            return new ArrayParameterOption<UInt32>(value, name, overlaySource);
        }

        public override void SetDefault()
        {
            if (Options.Count > 0)
            {
                SelectedOption = Options.First();
            }
            else
            {
                SelectedOption = null;
            }
        }
    }

    public class SceneModeParameter : ArrayParameter<CameraSceneMode>
    {
        public SceneModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.SceneMode, "Scene mode")
        {
        }

        protected override ArrayParameterOption<CameraSceneMode> CreateOption(CameraSceneMode value)
        {
            string name = value.EnumerationToParameterName<CameraSceneMode>();
            string overlaySource = "Assets/Icons/overlay.scenemode." + value.ToString().ToLower() + ".png";

            return new ArrayParameterOption<CameraSceneMode>(value, name, overlaySource);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption<CameraSceneMode> i in Options)
            {
                if (i.Value == CameraSceneMode.Auto || i == Options.Last())
                {
                    SelectedOption = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                SelectedOption = null;
            }
        }
    }

    public class FlashModeParameter : ArrayParameter<FlashMode>
    {
        public FlashModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashMode, "Flash mode")
        {
        }

        protected override ArrayParameterOption<FlashMode> CreateOption(FlashMode value)
        {
            string name = value.EnumerationToParameterName<FlashMode>();
            string overlaySource = "Assets/Icons/overlay.flashmode." + value.ToString().ToLower() + ".png";

            return new ArrayParameterOption<FlashMode>(value, name, overlaySource);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption<FlashMode> i in Options)
            {
                if (i.Value == FlashMode.Auto)
                {
                    SelectedOption = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                SelectedOption = null;
            }
        }
    }

    public class FocusIlluminationModeParameter : ArrayParameter<FocusIlluminationMode>
    {
        private ArrayParameterOption<FocusIlluminationMode> _defaultOption;

        public FocusIlluminationModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FocusIlluminationMode, "Focus illumination mode")
        {
        }

        protected override ArrayParameterOption<FocusIlluminationMode> CreateOption(FocusIlluminationMode value)
        {
            string name = value.EnumerationToParameterName<FocusIlluminationMode>();

            return new ArrayParameterOption<FocusIlluminationMode>(value, name);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption<FocusIlluminationMode> i in Options)
            {
                if (i.Value == FocusIlluminationMode.Auto)
                {
                    SelectedOption = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                SelectedOption = null;
            }
        }
    }

    public class WhiteBalancePresetParameter : ArrayParameter<WhiteBalancePreset>
    {
        public WhiteBalancePresetParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.WhiteBalancePreset, "White balance preset")
        {
        }

        protected override ArrayParameterOption<WhiteBalancePreset> CreateOption(WhiteBalancePreset value)
        {
            string name = value.EnumerationToParameterName<WhiteBalancePreset>();
            string overlaySource = "Assets/Icons/overlay.whitebalancepreset." + value.ToString().ToLower() + ".png";

            return new ArrayParameterOption<WhiteBalancePreset>(value, name, overlaySource);
        }

        public override void SetDefault()
        {
            // todo auto does not exist in the enumeration, which item to set?
        }
    }

    public class AutoFocusRangeParameter : ArrayParameter<AutoFocusRange>
    {
        public AutoFocusRangeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraGeneralProperties.AutoFocusRange, "Auto focus range")
        {
        }

        protected override ArrayParameterOption<AutoFocusRange> CreateOption(AutoFocusRange value)
        {
            string name = value.EnumerationToParameterName<AutoFocusRange>();

            return new ArrayParameterOption<AutoFocusRange>(value, name);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption<AutoFocusRange> i in Options)
            {
                if (i.Value == AutoFocusRange.Normal)
                {
                    SelectedOption = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                SelectedOption = null;
            }
        }
    }
}