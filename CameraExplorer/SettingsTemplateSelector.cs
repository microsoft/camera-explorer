/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

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