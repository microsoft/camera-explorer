/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
            if (_dataContext.Device == null)
            {
                NavigationService.GoBack();
            }
            else
            {
                _bitmap.SetSource(_dataContext.ImageStream);

                image.Source = _bitmap;
            }

            base.OnNavigatedTo(e);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                _dataContext.ImageStream.Position = 0;

                MediaLibrary library = new MediaLibrary();

                library.SavePictureToCameraRoll("CameraExplorer_" + DateTime.Now.ToString() + ".jpg", _dataContext.ImageStream);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Saving picture to camera roll failed");
            }

            NavigationService.GoBack();
        }
    }
}