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
        private bool _capturing = false;

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
                ShowProgress("Initializing camera...");

                await _dataContext.InitializeCamera(CameraSensorLocation.Back);

                HideProgress();
            }

            videoBrush.RelativeTransform = new CompositeTransform()
            {
                CenterX = 0.5,
                CenterY = 0.5,
                Rotation = _dataContext.Device.SensorLocation == CameraSensorLocation.Back ? _dataContext.Device.SensorRotationInDegrees : - _dataContext.Device.SensorRotationInDegrees
            };

            videoBrush.SetSource(_dataContext.Device);

            overlayComboBox.Opacity = 1;

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            overlayComboBox.Opacity = 0;

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            base.OnNavigatingFrom(e);
        }

        private void SetScreenButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
            {
                b.IsEnabled = enabled;
            }
        }

        private void SetCameraButtonsEnabled(bool enabled)
        {
            if (enabled)
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed += ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed += ShutterKeyPressed;
            }
            else
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed -= ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed -= ShutterKeyPressed;
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private async void sensorButton_Click(object sender, EventArgs e)
        {
            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            ShowProgress("Initializing camera...");

            videoBrush.Opacity = 0.25;

            overlayComboBox.Opacity = 0;

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

            HideProgress();

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);
        }

        private async void captureButton_Click(object sender, EventArgs e)
        {
            await AutoFocus();

            await Capture();
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

        private async Task AutoFocus()
        {
            if (!_capturing && PhotoCaptureDevice.IsFocusSupported(_dataContext.Device.SensorLocation))
            {
                SetScreenButtonsEnabled(false);
                SetCameraButtonsEnabled(false);

                await _dataContext.Device.FocusAsync();

                SetScreenButtonsEnabled(true);
                SetCameraButtonsEnabled(true);

                _capturing = false;
            }
        }

        private async Task Capture()
        {
            if (!_capturing)
            {
                _capturing = true;

                MemoryStream stream = new MemoryStream();

                CameraCaptureSequence sequence = _dataContext.Device.CreateCaptureSequence(1);
                sequence.Frames[0].CaptureStream = stream.AsOutputStream();

                await _dataContext.Device.PrepareCaptureSequenceAsync(sequence);
                await sequence.StartCaptureAsync();

                _dataContext.ImageStream = stream;

                _capturing = false;

                NavigationService.Navigate(new Uri("/PreviewPage.xaml", UriKind.Relative));
            }
        }

        private async void ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            await AutoFocus();
        }

        private async void ShutterKeyPressed(object sender, EventArgs e)
        {
            await Capture();
        }
    }
}