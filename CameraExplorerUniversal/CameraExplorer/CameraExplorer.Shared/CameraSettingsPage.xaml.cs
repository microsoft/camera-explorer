using CameraExplorer.ViewModels;
using ImageSequencer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CameraExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CameraSettingsPage : Page
    {
        private CameraSettingsViewModel _viewModel;
        public CameraSettingsPage()
        {
            this.InitializeComponent();
            var NavigationHelper = new NavigationHelper(this);
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            _viewModel = new CameraSettingsViewModel(CameraSession.Instance.MediaCapture, CameraSession.Instance.Settings);
            DataContext = _viewModel;
            ExposureCompensationSlider.Minimum = _viewModel.ExposureCompensationControl.Min;
            ExposureCompensationSlider.Maximum = _viewModel.ExposureCompensationControl.Max;
            ExposureTimeSlider.Minimum = _viewModel.ExposureTimeControl.Min;
            ExposureTimeSlider.Maximum = _viewModel.ExposureTimeControl.Max;
        }
    }
}
