﻿using System.Windows;
using System.Windows.Controls;

namespace CameraExplorer
{
    public class OverlayTemplateSelector : ContentControl
    {
        public DataTemplate ParameterWithoutOverlayTemplate { get; set; }
        public DataTemplate ParameterWithOverlayTemplate { get; set; }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ContentTemplate = SelectTemplate(newContent);

            base.OnContentChanged(oldContent, newContent);
        }

        public DataTemplate SelectTemplate(object item)
        {
            if ((item as Parameter).ImageSource != null)
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
