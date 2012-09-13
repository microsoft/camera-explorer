using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;

namespace CameraExplorer
{
    public partial class PreviewPage : PhoneApplicationPage
    {
        CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;

        BitmapImage _bitmap = new BitmapImage();

        public PreviewPage()
        {
            InitializeComponent();

            DataContext = _dataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _bitmap.SetSource(_dataContext.ImageStream);

            image.Source = _bitmap;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _dataContext.ImageStream = null;

            base.OnNavigatingFrom(e);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                MediaLibrary library = new MediaLibrary();

                library.SavePictureToCameraRoll("CameraExplorer", _dataContext.ImageStream);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Saving picture to camera roll failed");
            }

            NavigationService.GoBack();
        }
    }
}