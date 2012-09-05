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

namespace CameraExplorer
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton();

        public SettingsPage()
        {
            InitializeComponent();

            DataContext = _dataContext;

            _dataContext.Settings.Device = _dataContext.Device;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            _dataContext.Settings.Refresh();
        }
    }
}