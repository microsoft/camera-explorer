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
            if ((item as Parameter).Supported)
            {
                if (item is IsoParameter ||
                    item is SceneModeParameter ||
                    item is FlashModeParameter ||
                    item is WhiteBalancePresetParameter ||
                    item is AutoFocusRangeParameter ||
                    item is FocusIlluminationModeParameter ||
                    item is ExposureTimeParameter ||
                    item is PreviewResolutionParameter ||
                    item is CaptureResolutionParameter)
                {
                    return ArrayParameterTemplate;
                }
                else if (item is ManualWhiteBalanceParameter ||
                    item is FlashPowerParameter ||
                    item is ExposureCompensationParameter)
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
