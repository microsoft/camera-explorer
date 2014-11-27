/*
 * Copyright (c) 2012-2014 Microsoft Mobile. All rights reserved.
 * See the license file delivered with this project for more information.
 */

using System.Windows;
using System.Windows.Controls;

namespace CameraExplorer
{
    /// <summary>
    /// Settings template selector selects the appropriate UI control tempalate for a parameter.
    /// See the SettingsPage.xaml file for the template declarations.
    /// </summary>
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

        /// <summary>
        /// If parameter is not supported or modifiable, it is not displayed in the UI, and thus
        /// the unsupported parameter template is used. If Parameter is supported and modifiable,
        /// then the decision between the template is done on basis of the type of the parameter.
        /// </summary>
        /// <param name="item">Parameter instance</param>
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