using System.Windows;
using System.Windows.Controls;

namespace CameraExplorer
{
    public class SettingsTemplateSelector : ContentControl
    {
        public DataTemplate ArrayParameterTemplate { get; set; }
        public DataTemplate RangeParameterTemplate { get; set; }
        public DataTemplate UnsupportedParameterTemplate { get; set; }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ContentTemplate = SelectTemplate(newContent);

            base.OnContentChanged(oldContent, newContent);
        }

        public DataTemplate SelectTemplate(object item)
        {
            Parameter parameter = item as Parameter;

            if (parameter.Supported && parameter.Modifiable)
            {
                if (parameter is ArrayParameter)
                {
                    return ArrayParameterTemplate;
                }
                else
                {
                    return RangeParameterTemplate;
                }
            }
            else
            {
                return UnsupportedParameterTemplate;
            }
        }
    }
}