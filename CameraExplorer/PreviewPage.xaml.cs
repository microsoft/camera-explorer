using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;

namespace CameraExplorer
{
    public partial class PreviewPage : PhoneApplicationPage
    {
        private CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;
        private BitmapImage _bitmap = new BitmapImage();

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

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                MediaLibrary library = new MediaLibrary();

                library.SavePictureToCameraRoll("CameraExplorer.jpg", _dataContext.ImageStream);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Saving picture to camera roll failed");
            }

            NavigationService.GoBack();
        }
    }
}