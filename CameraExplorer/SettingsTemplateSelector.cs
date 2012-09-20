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
                if (parameter is IsoParameter ||
                    parameter is SceneModeParameter ||
                    parameter is FlashModeParameter ||
                    parameter is WhiteBalancePresetParameter ||
                    parameter is AutoFocusRangeParameter ||
                    parameter is FocusIlluminationModeParameter ||
                    parameter is ExposureTimeParameter ||
                    parameter is PreviewResolutionParameter ||
                    parameter is CaptureResolutionParameter)
                {
                    return ArrayParameterTemplate;
                }
                else if (parameter is ManualWhiteBalanceParameter ||
                    parameter is FlashPowerParameter ||
                    parameter is ExposureCompensationParameter)
                {
                    return RangeParameterTemplate;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return UnsupportedParameterTemplate;
            }
        }
    }
}
