/*
 * Copyright (c) 2012-2014 Microsoft Mobile. All rights reserved.
 * See the license file delivered with this project for more information.
 */

using System.Windows;
using System.Windows.Controls;

namespace CameraExplorer
{
    /// <summary>
    /// Overlay template selector selects the overlay template for a parameter. See the
    /// MainPage.xaml file for the template declarations.
    /// </summary>
    public class OverlayTemplateSelector : ContentControl
    {
        public DataTemplate ParameterWithoutOverlayTemplate { get; set; }
        public DataTemplate ParameterWithOverlayTemplate { get; set; }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ContentTemplate = SelectTemplate(newContent);

            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// If parameter has OverlaySource set then a template with an Image XAML control is selected,
        /// otherwise a template without an Image control is used.
        /// </summary>
        /// <param name="item">Parameter instance</param>
        public DataTemplate SelectTemplate(object item)
        {
            var parameter = item as Parameter;
            if (parameter != null && parameter.OverlaySource != null)
            {
                return ParameterWithOverlayTemplate;
            }
            else
            {
                return ParameterWithoutOverlayTemplate;
            }
        }
    }
}
