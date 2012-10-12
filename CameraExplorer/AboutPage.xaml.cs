using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml.Linq;

namespace MapExplorer
{
    public partial class AboutPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;

        public AboutPage()
        {
            InitializeComponent();

            versionTextBox.Text = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_dataContext.Device == null)
            {
                NavigationService.GoBack();
            }

            base.OnNavigatedTo(e);
        }
    }
}