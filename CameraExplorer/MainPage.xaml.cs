using System;
using System.Collections.Generic;
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
using Microsoft.Phone.Shell;
using CameraExplorer.Resources;
using Microsoft.Devices;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Windows.Media.Imaging;

namespace CameraExplorer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;
        private ProgressIndicator _progressIndicator = new ProgressIndicator();

        public MainPage()
        {
            InitializeComponent();

            ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
            menuItem.Text = "about";
            ApplicationBar.MenuItems.Add(menuItem);
            menuItem.Click += new EventHandler(aboutMenuItem_Click);

            DataContext = _dataContext;

            _progressIndicator.IsIndeterminate = true;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_dataContext.Device == null)
            {
                SetButtonsEnabled(false);

                ShowProgress("Initializing camera...");

                await _dataContext.InitializeCamera(CameraSensorLocation.Back);

                HideProgress();

                if (PhotoCaptureDevice.IsFocusSupported(_dataContext.Device.SensorLocation))
                {
                    await _dataContext.Device.FocusAsync();
                }

                SetButtonsEnabled(true);
            }

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device.SensorLocation == CameraSensorLocation.Back ? _dataContext.Device.SensorRotationInDegrees : - _dataContext.Device.SensorRotationInDegrees
            };

            videoBrush.SetSource(_dataContext.Device);

            overlayComboBox.Opacity = 1;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            overlayComboBox.Opacity = 0;

            base.OnNavigatingFrom(e);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
            {
                b.IsEnabled = enabled;
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private async void sensorButton_Click(object sender, EventArgs e)
        {
            ShowProgress("Initializing camera...");

            videoBrush.Opacity = 0.25;

            overlayComboBox.Opacity = 0;

            SetButtonsEnabled(false);

            CameraSensorLocation currentSensorLocation = _dataContext.Device.SensorLocation;

            _dataContext.UnitializeCamera();

            IReadOnlyList<CameraSensorLocation> sensorLocations = PhotoCaptureDevice.AvailableSensorLocations;

            if (currentSensorLocation == sensorLocations[1])
            {
                await _dataContext.InitializeCamera(sensorLocations[0]);
            }
            else
            {
                await _dataContext.InitializeCamera(sensorLocations[1]);
            }

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device.SensorLocation == CameraSensorLocation.Back ? _dataContext.Device.SensorRotationInDegrees : _dataContext.Device.SensorRotationInDegrees + 180
            };

            videoBrush.SetSource(_dataContext.Device);
            videoBrush.Opacity = 1;

            overlayComboBox.Opacity = 1;

            SetButtonsEnabled(true);

            HideProgress();
        }

        private async void captureButton_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);

            if (PhotoCaptureDevice.IsFocusSupported(_dataContext.Device.SensorLocation))
            {
                await _dataContext.Device.FocusAsync();
            }

            MemoryStream stream = new MemoryStream();

            CameraCaptureSequence sequence = _dataContext.Device.CreateCaptureSequence(1);
            sequence.Frames[0].CaptureStream = stream.AsOutputStream();

            await _dataContext.Device.PrepareCaptureSequenceAsync(sequence);
            await sequence.StartCaptureAsync();

            _dataContext.ImageStream = stream;

            await _dataContext.Device.ResetFocusAsync();

            NavigationService.Navigate(new Uri("/PreviewPage.xaml", UriKind.Relative));

            SetButtonsEnabled(true);
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void ShowProgress(String msg)
        {
            _progressIndicator.Text = msg;
            _progressIndicator.IsVisible = true;

            SystemTray.SetProgressIndicator(this, _progressIndicator);
        }

        private void HideProgress()
        {
            _progressIndicator.IsVisible = false;

            SystemTray.SetProgressIndicator(this, _progressIndicator);
        }
    }
}