using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace CameraExplorer

{
    public class InverseConverter : IValueConverter
    {
            public object Convert(
                object value,
                Type targetType,
                object parameter,
                string language)
            {
                return !(bool)value;
            }

            public object ConvertBack(
                object value,
                Type targetType,
                object parameter,
                string language)
            {
                return !(bool)value;
            }
        
    }
}
