using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Media.Capture;
using System.Collections;

namespace CameraExplorer
{
    class PhotoCaptureDeviceParser
    {
        // device.PreviewResolution
        // PhotoCaptureDevice.GetAvailablePreviewResolutions(device.SensorLocation)

        // device.CaptureResolution
        // PhotoCaptureDevice.GetAvailableCaptureResolutions(device.SensorLocation)

        // PhotoCaptureDevice.IsFocusRegionSupported(device.SensorLocation)
        // PhotoCaptureDevice.IsFocusSupported(device.SensorLocation)

        // KnownCameraPhotoProperties.ExposureCompensation
        // KnownCameraPhotoProperties.ExposureTime
        // KnownCameraPhotoProperties.FlashMode
        // KnownCameraPhotoProperties.FlashPower
        // KnownCameraPhotoProperties.FocusIlluminationMode
        // KnownCameraPhotoProperties.Iso
        // KnownCameraPhotoProperties.LockedAutoFocusParameters
        // KnownCameraPhotoProperties.ManualWhiteBalance
        // KnownCameraPhotoProperties.SceneMode
        // KnownCameraPhotoProperties.WhiteBalancePreset
        // KnownCameraGeneralProperties.AutoFocusRange

        // KnownCameraGeneralProperties.EncodeWithOrientation
        // KnownCameraGeneralProperties.IsShutterSoundEnabledByUser
        // KnownCameraGeneralProperties.IsShutterSoundRequiredForRegion
        // KnownCameraGeneralProperties.ManualFocusPosition
        // KnownCameraGeneralProperties.PlayShutterSoundOnCapture
        // KnownCameraGeneralProperties.SpecifiedCaptureOrientation

        public static async void Parse(PhotoCaptureDevice device, Collection<Settings.Parameter> collection)
        {
            try
            {
                collection.Add(ParseExposureCompensation(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse exposure compensation.");
            }

            try
            {
                collection.Add(ParseExposureTime(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse exposure time.");
            }

            try
            {
                collection.Add(ParseFlashMode(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse flash mode.");
            }

            try
            {
                collection.Add(ParseFlashPower(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse flash power.");
            }

            try
            {
                collection.Add(ParseFocusIlluminationMode(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse focus illumination mode.");
            }

            try
            {
                collection.Add(ParseIso(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse ISO.");
            }

            try
            {
                collection.Add(ParseLockedAutofocusParameters(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse locked autofocus parameters.");
            }

            try
            {
                collection.Add(ParseManualWhiteBalance(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse manual white balance.");
            }

            try
            {
                collection.Add(ParseSceneMode(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse scene mode.");
            }

            try
            {
                collection.Add(ParseWhiteBalancePreset(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse white balance preset.");
            }

            try
            {
                collection.Add(ParseAutoFocusRange(device));
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Unable to parse autofocus range.");
            }
        }

        static Settings.Parameter ParseSensorLocations(PhotoCaptureDevice device)
        {
            Settings.ArrayParameter parameter = new Settings.ArrayParameter() { DisplayName = "Sensor location" };

            IReadOnlyList<CameraSensorLocation> sensorLocations = PhotoCaptureDevice.AvailableSensorLocations;

            for (int i = 0; i < sensorLocations.Count; i++)
            {
                CameraSensorLocation location = sensorLocations[i];
                Settings.ArrayParameter.Value parameterValue = new Settings.ArrayParameter.Value();

                if (location == CameraSensorLocation.Back)
                {
                    parameterValue.DisplayName = "Back";
                }
                else if (location == CameraSensorLocation.Front)
                {
                    parameterValue.DisplayName = "Front";
                }
                else
                {
                    parameterValue.DisplayName = "Unknown";
                }

                parameterValue.Action = async () =>
                    {
                        DataContext.Singleton().UnitializeCamera();
                        await DataContext.Singleton().InitializeCamera(location);
                    };

                parameter.PossibleValues.Add(parameterValue);

                if (device.SensorLocation == location)
                {
                    parameter.CurrentValue = parameterValue;
                }
            }

            return parameter;
        }

        static Settings.Parameter ParseExposureTime(PhotoCaptureDevice device)
        {
            List<Tuple<UInt32, string>> exposureTimes = new List<Tuple<UInt32, string>>();
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 16000, "1/16000 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 8000, "1/8000 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 4000, "1/4000 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 2000, "1/2000 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 1000, "1/1000 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 500, "1/500 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 250, "1/250 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 125, "1/125 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 60, "1/60 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 30, "1/30 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 15, "1/15 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 8, "1/8 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 4, "1/4 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000 / 2 , "1/2 s"));
            exposureTimes.Add(new Tuple<UInt32, string>(1000000, "1 s"));

            Func<object, object, IReadOnlyList<object>> vtaf = (object min, object max) =>
            {
                UInt32 minUInt32 = (UInt32)min;
                UInt32 maxUInt32 = (UInt32)max;

                List<object> list = new List<object>();

                foreach (Tuple<UInt32, string> i in exposureTimes)
                {
                    if (i.Item1 >= minUInt32 && i.Item1 <= maxUInt32)
                    {
                        list.Add(i.Item1);
                    }
                }

                return list;
            };

            Func<object, string> vnf = (object value) =>
            {
                UInt32 valueUInt32 = (UInt32)value;
                string name = null;

                foreach (Tuple<UInt32, string> i in exposureTimes)
                {
                    if (valueUInt32 == i.Item1)
                    {
                        name = i.Item2;
                        break;
                    }
                }

                return name;
            };

            return ParseRangePropertyAsArray(device, KnownCameraPhotoProperties.ExposureTime, "Exposure time", vtaf, vnf);
        }

        static Settings.Parameter ParseExposureCompensation(PhotoCaptureDevice device)
        {
            return ParseRangeProperty(device, KnownCameraPhotoProperties.ExposureCompensation, "Exposure compensation", "EV");
        }

        static Settings.Parameter ParseFlashMode(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
                {
                    FlashState v = (FlashState)(UInt32)o;

                    if (v == FlashState.Auto)
                        return "Auto";
                    else if (v == FlashState.Off)
                        return "Off";
                    else if (v == FlashState.On)
                        return "On";
                    else
                        return "Unknown";
                };

            return ParseArrayProperty(device, KnownCameraPhotoProperties.FlashMode, "Flash mode", f);
        }

        static Settings.Parameter ParseFlashPower(PhotoCaptureDevice device)
        {
            return ParseRangeProperty(device, KnownCameraPhotoProperties.FlashPower, "Flash power", "");
        }

        static Settings.Parameter ParseFocusIlluminationMode(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
                {
                    FocusIlluminationMode v = (FocusIlluminationMode)(UInt32)o;

                    if (v == FocusIlluminationMode.Auto)
                        return "Auto";
                    else if (v == FocusIlluminationMode.Off)
                        return "Off";
                    else if (v == FocusIlluminationMode.On)
                        return "On";
                    else
                        return "Unknown";
                };

            return ParseArrayProperty(device, KnownCameraPhotoProperties.FocusIlluminationMode, "Focus illumination mode", f);
        }

        static Settings.Parameter ParseIso(PhotoCaptureDevice device)
        {
            Func<object, object, IReadOnlyList<object>> vtaf = (object min, object max) =>
            {
                UInt32 minUInt32 = (UInt32)min;
                UInt32 maxUInt32 = (UInt32)max;

                List<object> list = new List<object>();

                minUInt32 = minUInt32 > 100 ? minUInt32 : 100;

                while (minUInt32 <= maxUInt32)
                {
                    list.Add(minUInt32);
                    minUInt32 *= 2;
                }

                return list;
            };

            Func<object, string> vnf = (object value) =>
                {
                    return value.ToString();
                };

            return ParseRangePropertyAsArray(device, KnownCameraPhotoProperties.Iso, "ISO", vtaf, vnf);
        }

        static Settings.Parameter ParseLockedAutofocusParameters(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
                {
                    AutoFocusParameters v = (AutoFocusParameters)(UInt32)o;

                    if (v == AutoFocusParameters.None)
                        return "None";
                    if (v == AutoFocusParameters.Focus)
                        return "Focus";
                    else if (v == AutoFocusParameters.Exposure)
                        return "Exposure";
                    else if (v == AutoFocusParameters.WhiteBalance)
                        return "White balance";
                    else
                        return "Unknown";
                };

            return ParseArrayProperty(device, KnownCameraPhotoProperties.LockedAutoFocusParameters, "Locked autofocus parameters", f);
        }

        static Settings.Parameter ParseManualWhiteBalance(PhotoCaptureDevice device)
        {
            return ParseRangeProperty(device, KnownCameraPhotoProperties.ManualWhiteBalance, "Manual white balance", "K");
        }

        static Settings.Parameter ParseSceneMode(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
            {
                CameraSceneMode v = (CameraSceneMode)(UInt32)o;

                if (v == CameraSceneMode.Auto)
                    return "Auto";
                if (v == CameraSceneMode.Macro)
                    return "Macro";
                if (v == CameraSceneMode.Portrait)
                    return "Portrait";
                if (v == CameraSceneMode.Sport)
                    return "Sport";
                if (v == CameraSceneMode.Snow)
                    return "Snow";
                if (v == CameraSceneMode.Night)
                    return "Night";
                if (v == CameraSceneMode.Beach)
                    return "Beach";
                if (v == CameraSceneMode.Sunset)
                    return "Sunset";
                if (v == CameraSceneMode.Candlelight)
                    return "Candlelight";
                if (v == CameraSceneMode.Landscape)
                    return "Landscape";
                if (v == CameraSceneMode.NightPortrait)
                    return "Night portrait";
                if (v == CameraSceneMode.Backlit)
                    return "Backlit";
                else
                    return "Unknown";
            };

            return ParseArrayProperty(device, KnownCameraPhotoProperties.SceneMode, "Scene mode", f);
        }

        static Settings.Parameter ParseWhiteBalancePreset(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
            {
                WhiteBalancePreset v = (WhiteBalancePreset)(UInt32)o;

                if (v == WhiteBalancePreset.Cloudy)
                    return "Cloudy";
                if (v == WhiteBalancePreset.Daylight)
                    return "Daylight";
                if (v == WhiteBalancePreset.Flash)
                    return "Flash";
                if (v == WhiteBalancePreset.Fluorescent)
                    return "Fluorescent";
                if (v == WhiteBalancePreset.Tungsten)
                    return "Tungsten";
                if (v == WhiteBalancePreset.Candlelight)
                    return "Candlelight";
                else
                    return "Unknown";
            };

            return ParseArrayProperty(device, KnownCameraPhotoProperties.WhiteBalancePreset, "White balance preset", f);
        }

        static Settings.Parameter ParseAutoFocusRange(PhotoCaptureDevice device)
        {
            Func<object, string> f = (object o) =>
            {
                AutoFocusRange v = (AutoFocusRange)(UInt32)o;

                if (v == AutoFocusRange.Macro)
                    return "Macro";
                if (v == AutoFocusRange.Normal)
                    return "Normal";
                if (v == AutoFocusRange.Full)
                    return "Full";
                if (v == AutoFocusRange.Hyperfocal)
                    return "Hyperfocal";
                if (v == AutoFocusRange.Infinity)
                    return "Infinity";
                else
                    return "Unknown";
            };

            return ParseArrayProperty(device, KnownCameraGeneralProperties.AutoFocusRange, "Autofocus range", f);
        }

        static Settings.Parameter ParseArrayProperty(PhotoCaptureDevice device, Guid guid, string displayName, Func<object, string> valueDisplayNameFunc)
        {
            Settings.ArrayParameter parameter = new Settings.ArrayParameter();
            parameter.DisplayName = displayName;

            IReadOnlyList<object> possibleValues = PhotoCaptureDevice.GetSupportedPropertyValues(device.SensorLocation, guid);
            object value = device.GetProperty(guid);

            for (int i = 0; i < possibleValues.Count; i++)
            {
                object possibleValue = possibleValues[i];

                Settings.ArrayParameter.Value arrayValue = new Settings.ArrayParameter.Value();
                arrayValue.DisplayName = valueDisplayNameFunc(possibleValue);
                arrayValue.Action = () =>
                {
                    device.SetProperty(guid, possibleValue);
                };

                if (possibleValue.Equals(value))
                {
                    parameter.CurrentValue = arrayValue;
                }

                parameter.PossibleValues.Add(arrayValue);
            }

            return parameter;
        }

        static Settings.Parameter ParseRangeProperty(PhotoCaptureDevice device, Guid guid, string displayName, string unit)
        {
            Settings.RangeParameter parameter = new Settings.RangeParameter();
            parameter.DisplayName = displayName;
            parameter.Unit = unit;

            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(device.SensorLocation, guid);
            object value = device.GetProperty(guid);

            if (range.Min.Equals(range.Max))
            {
                throw new Exception("Guid=" + guid + " range min=" + range.Min + " is equal to range max=" + range.Max);
            }

            parameter.Min = range.Min;
            parameter.Max = range.Max;
            parameter.CurrentValue = value;

            parameter.Action = () =>
                {
                    device.SetProperty(guid, parameter.CurrentValue);
                };

            return parameter;
        }

        static Settings.Parameter ParseRangePropertyAsArray(PhotoCaptureDevice device, Guid guid, string displayName,
            Func<object, object, IReadOnlyList<object>> rangeToArrayFunc, Func<object, string> valueToNameFunc)
        {
            Settings.ArrayParameter parameter = new Settings.ArrayParameter();
            parameter.DisplayName = displayName;

            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(device.SensorLocation, guid);
            object value = device.GetProperty(guid);

            IReadOnlyList<object> possibleValues = rangeToArrayFunc(range.Min, range.Max);

            for (int i = 0; i < possibleValues.Count; i++)
            {
                object possibleValue = possibleValues[i];

                Settings.ArrayParameter.Value arrayValue = new Settings.ArrayParameter.Value();
                arrayValue.DisplayName = valueToNameFunc(possibleValue);
                arrayValue.Action = () =>
                {
                    device.SetProperty(guid, possibleValue);
                };

                if (possibleValue.Equals(value))
                {
                    parameter.CurrentValue = arrayValue;
                }

                parameter.PossibleValues.Add(arrayValue);
            }

            return parameter;
        }
    }
}