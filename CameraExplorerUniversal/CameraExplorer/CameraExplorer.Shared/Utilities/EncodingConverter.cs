using CameraExplorer.Models;
using CameraExplorer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Data;

namespace CameraExplorer
{
    public class EncodingConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            if (value.GetType() == typeof(ImageEncodingProperties))
            {
                var v = value as ImageEncodingProperties;
                return new CaptureResolution(v.Width, v.Height);
            }
            if (value.GetType() == typeof(VideoEncodingProperties))
            {
                var v = value as VideoEncodingProperties;
                return new CaptureResolution(v.Width, v.Height);
            }
           
            List<object> targetList = new List<object>();
            var sourceList = value as IEnumerable;
            foreach (var item in sourceList)
            {
                targetList.Add(Convert(item as object, targetType, parameter, language));
            }
            
            return targetList;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            var encodings = CameraUtilities.GetAvailableCaptureResolutions(CameraSession.Instance.MediaCapture);
            CaptureResolution res = value as CaptureResolution;
            foreach (var encoding in encodings)
            {
                if (encoding.GetType() == typeof(ImageEncodingProperties))
                {
                    var v = encoding as ImageEncodingProperties;
                    if (res.width == v.Width && res.height == v.Height)
                        return encoding;
                }
                if (encoding.GetType() == typeof(VideoEncodingProperties))
                {
                    var v = encoding as VideoEncodingProperties;
                    if (res.width == v.Width && res.height == v.Height)
                        return encoding;
                }
            }
            return "Error";
        }
    }
}
