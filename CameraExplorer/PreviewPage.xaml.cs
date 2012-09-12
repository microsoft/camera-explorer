using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CameraExplorer
{
    public partial class PreviewPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;

        public PreviewPage()
        {
            InitializeComponent();

            DataContext = _dataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            image.Source = _dataContext.Image;

            base.OnNavigatedTo(e);
        }
    }
}