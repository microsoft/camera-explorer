using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Windows.Phone.Media.Capture;
using Microsoft.Phone.Shell;

namespace CameraExplorer
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;

        public SettingsPage()
        {
            InitializeComponent();

            DataContext = _dataContext;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_dataContext.Device == null)
            {
                NavigationService.GoBack();
            }

            base.OnNavigatedTo(e);
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            SetScreenButtonsEnabled(false);

            foreach (Parameter i in _dataContext.Settings.Parameters)
            {
                i.SetDefault();
            }

            SetScreenButtonsEnabled(true);
        }

        private void SetScreenButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
            {
                b.IsEnabled = enabled;
            }
        }
    }
}