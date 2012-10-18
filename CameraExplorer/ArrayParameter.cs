using Microsoft.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Phone.Media.Capture;

namespace CameraExplorer
{
    public class ArrayParameterEnumerator : IEnumerator<ArrayParameterOption>
    {
        private ArrayParameter _arrayParameter;
        private int _count;
        private int _index = -1;

        public ArrayParameterEnumerator(ArrayParameter arrayParameter, int count)
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

        ArrayParameterOption IEnumerator<ArrayParameterOption>.Current
        {
            get
            {
                return (ArrayParameterOption)Current;
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

    public class ArrayParameterOption
    {
        private dynamic _value;
        private string _name;
        private string _overlaySource;

        public ArrayParameterOption(dynamic value, string name, string overlaySource = null)
        {
            _value = value;
            _name = name;
            _overlaySource = overlaySource;
        }

        public dynamic Value
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

    public abstract class ArrayParameter : Parameter, IReadOnlyCollection<ArrayParameterOption>
    {
        private List<ArrayParameterOption> _options = new List<ArrayParameterOption>();
        private ArrayParameterOption _selectedOption;
        private Guid _propertyId;
        private bool _refreshing = false;

        public ArrayParameter(PhotoCaptureDevice device, string name)
            : base(device, name)
        {
        }

        public ArrayParameter(PhotoCaptureDevice device, Guid propertyId, string name)
            : base(device, name)
        {
            _propertyId = propertyId;
        }

        public override void Refresh()
        {
            _refreshing = true;

            _options.Clear();

            _selectedOption = null;

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

            _refreshing = false;
        }

        public ArrayParameterOption Option(int index)
        {
            return _options[index];
        }

        public ArrayParameterOption SelectedOption
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
                    if (!(_refreshing && _selectedOption == null))
                    {
                        SetOption(value);
                    }

                    _selectedOption = value;

                    OverlaySource = _selectedOption.OverlaySource;

                    if (!(_refreshing && _selectedOption == null))
                    {
                        NotifyPropertyChanged("SelectedOption");
                        NotifyPropertyChanged("OverlaySource");
                    }
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

        public IEnumerator<ArrayParameterOption> GetEnumerator()
        {
            return new ArrayParameterEnumerator(this, _options.Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayParameterEnumerator(this, _options.Count);
        }

        protected Guid PropertyId
        {
            get
            {
                return _propertyId;
            }
        }

        protected List<ArrayParameterOption> Options
        {
            get
            {
                return _options;
            }
        }

        protected abstract void PopulateOptions();

        protected abstract void SetOption(ArrayParameterOption option);
    }

    public class CaptureResolutionParameter : ArrayParameter
    {
        public CaptureResolutionParameter(PhotoCaptureDevice device)
            : base(device, "Capture resolution")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<Windows.Foundation.Size> supportedValues = PhotoCaptureDevice.GetAvailableCaptureResolutions(Device.SensorLocation);
            Windows.Foundation.Size value = Device.CaptureResolution;

            ArrayParameterOption option = null;

            foreach (Windows.Foundation.Size i in supportedValues)
            {
                option = new ArrayParameterOption(i, i.Width + " x " + i.Height);

                Options.Add(option);

                if (i.Equals(value))
                {
                    SelectedOption = option;
                }
            }
        }

        protected async override void SetOption(ArrayParameterOption option)
        {
            if (Modifiable)
            {
                Modifiable = false;

                await Device.SetCaptureResolutionAsync((Windows.Foundation.Size)option.Value);

                Modifiable = true;
            }
        }

        public override void SetDefault()
        {
            SelectedOption = Options.Count > 0 ? Options.First() : null;
        }
    }

    public class ExposureTimeParameter : ArrayParameter
    {
        public ExposureTimeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.ExposureTime, "Exposure time")
        {
        }

        protected override void PopulateOptions()
        {
            ArrayParameterOption option = new ArrayParameterOption(null, "Auto", "Assets/Icons/overlay.exposuretime.auto.png");
            ArrayParameterOption selectedOption = option;

            Options.Add(option);

            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, KnownCameraPhotoProperties.ExposureTime);
            object value = Device.GetProperty(PropertyId);
            UInt32[] standardValues = { /* 16000, 8000, 4000,*/ 2000, 1000, 500, 250, 125, 60, 30, 15, 8, 4, 2, 1 };

            UInt32 min = (UInt32)range.Min;
            UInt32 max = (UInt32)range.Max;

            foreach (UInt32 i in standardValues)
            {
                UInt32 usecs = 1000000 / i;

                if (usecs >= min && usecs <= max)
                {
                    option = new ArrayParameterOption(usecs, "1 / " + i.ToString() + " s", "Assets/Icons/overlay.exposuretime." + i.ToString() + ".png");

                    Options.Add(option);

                    if (selectedOption == null && usecs.Equals(value))
                    {
                        selectedOption = option;
                    }
                }
            }

            SelectedOption = selectedOption;
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
        }

        public override void SetDefault()
        {
            SelectedOption = Options.Count > 0 ? Options.First() : null;
        }
    }

    public class IsoParameter : ArrayParameter
    {
        public IsoParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.Iso, "ISO")
        {
        }

        protected override void PopulateOptions()
        {
            ArrayParameterOption option = new ArrayParameterOption(null, "Auto", "Assets/Icons/overlay.iso.auto.png");
            ArrayParameterOption selectedOption = option;

            Options.Add(option);

            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);
            UInt32[] standardValues = { 100, 200, 400, 800, 1600, 3200 };

            UInt32 min = (UInt32)range.Min;
            UInt32 max = (UInt32)range.Max;

            foreach (UInt32 i in standardValues)
            {
                if (i >= min && i <= max)
                {
                    option = new ArrayParameterOption(i, "ISO " + i.ToString(), "Assets/Icons/overlay.iso." + i.ToString() + ".png");

                    Options.Add(option);

                    if (i.Equals(value))
                    {
                        selectedOption = option;
                    }
                }
            }

            SelectedOption = selectedOption;
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
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

    public class SceneModeParameter : ArrayParameter
    {
        public SceneModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.SceneMode, "Scene mode")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);

            foreach (dynamic i in supportedValues)
            {
                CameraSceneMode csm = (CameraSceneMode)i;

                ArrayParameterOption option = new ArrayParameterOption(csm, csm.EnumerationToParameterName<CameraSceneMode>(), "Assets/Icons/overlay.scenemode." + csm.ToString().ToLower() + ".png");

                Options.Add(option);

                if (i.Equals(value))
                {
                    SelectedOption = option;
                }
            }
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption i in Options)
            {
                if ((CameraSceneMode)i.Value == CameraSceneMode.Auto || i == Options.Last())
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

    public class FlashModeParameter : ArrayParameter
    {
        public FlashModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FlashMode, "Flash mode")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);

            foreach (dynamic i in supportedValues)
            {
                FlashMode fm = (FlashMode)i;

                ArrayParameterOption option = new ArrayParameterOption(fm, fm.EnumerationToParameterName<FlashMode>(), "Assets/Icons/overlay.flashmode." + fm.ToString().ToLower() + ".png");

                Options.Add(option);

                if (i.Equals(value))
                {
                    SelectedOption = option;
                }
            }
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, (FlashMode)option.Value);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption i in Options)
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

    public class FocusIlluminationModeParameter : ArrayParameter
    {
        public FocusIlluminationModeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.FocusIlluminationMode, "Focus illumination mode")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);

            foreach (dynamic i in supportedValues)
            {
                FocusIlluminationMode fim = (FocusIlluminationMode)i;

                ArrayParameterOption option = new ArrayParameterOption(fim, fim.EnumerationToParameterName<FocusIlluminationMode>());

                Options.Add(option);

                if (i.Equals(value))
                {
                    SelectedOption = option;
                }
            }
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption i in Options)
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

    public class WhiteBalancePresetParameter : ArrayParameter
    {
        public WhiteBalancePresetParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraPhotoProperties.WhiteBalancePreset, "White balance preset")
        {
        }

        protected override void PopulateOptions()
        {
            ArrayParameterOption option = new ArrayParameterOption(null, "Auto");
            ArrayParameterOption selectedOption = option;

            Options.Add(option);

            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);

            foreach (dynamic i in supportedValues)
            {
                WhiteBalancePreset wbp = (WhiteBalancePreset)i;

                option = new ArrayParameterOption(wbp, wbp.EnumerationToParameterName<WhiteBalancePreset>(), "Assets/Icons/overlay.whitebalancepreset." + wbp.ToString().ToLower() + ".png");

                Options.Add(option);

                if (i.Equals(value))
                {
                    selectedOption = option;
                }
            }

            SelectedOption = selectedOption;
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
        }

        public override void SetDefault()
        {
            SelectedOption = Options.Count > 0 ? Options.First() : null;
        }
    }

    public class AutoFocusRangeParameter : ArrayParameter
    {
        public AutoFocusRangeParameter(PhotoCaptureDevice device)
            : base(device, KnownCameraGeneralProperties.AutoFocusRange, "Auto focus range")
        {
        }

        protected override void PopulateOptions()
        {
            IReadOnlyList<object> supportedValues = PhotoCaptureDevice.GetSupportedPropertyValues(Device.SensorLocation, PropertyId);
            object value = Device.GetProperty(PropertyId);

            foreach (dynamic i in supportedValues)
            {
                AutoFocusRange afr = (AutoFocusRange)i;

                ArrayParameterOption option = new ArrayParameterOption(afr, afr.EnumerationToParameterName<AutoFocusRange>());

                Options.Add(option);

                if (i.Equals(value))
                {
                    SelectedOption = option;
                }
            }
        }

        protected override void SetOption(ArrayParameterOption option)
        {
            Device.SetProperty(PropertyId, option.Value);
        }

        public override void SetDefault()
        {
            bool found = false;

            foreach (ArrayParameterOption i in Options)
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