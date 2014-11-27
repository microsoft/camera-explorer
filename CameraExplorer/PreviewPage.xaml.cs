/*
 * Copyright (c) 2012-2014 Microsoft Mobile. All rights reserved.
 * See the license file delivered with this project for more information.
 */

using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CameraExplorer
{
    /// <summary>
    /// Preview page displays the captured photo from DataContext.ImageStream and
    /// has a button to save the image to phone's camera roll.
    /// </summary>
    public partial class PreviewPage : PhoneApplicationPage
    {
        private CameraExplorer.DataContext _dataContext = CameraExplorer.DataContext.Singleton;
        private BitmapImage _bitmap = new BitmapImage();

        public PreviewPage()
        {
            InitializeComponent();

            DataContext = _dataContext;
        }

        /// <summary>
        /// When navigating to this page, DataContext.ImageStream will be set as the source
        /// for the Image control in XAML. If ImageStream is null, application will navigate
        /// directly back to the main page.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_dataContext.ImageStream != null)
            {
                _bitmap.SetSource(_dataContext.ImageStream);
                image.Source = _bitmap;
            }
            else
            {
                NavigationService.GoBack();
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Clicking on the save button saves the photo in DataContext.ImageStream to media library
        /// camera roll. Once image has been saved, the application will navigate back to the main page.
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Reposition ImageStream to beginning, because it has been read already in the OnNavigatedTo method.
                _dataContext.ImageStream.Position = 0;

                MediaLibrary library = new MediaLibrary();
                library.SavePictureToCameraRoll("CameraExplorer_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg", _dataContext.ImageStream);
                
                // There should be no temporary file left behind
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var files = isolatedStorage.GetFileNames("CameraExplorer_*.jpg");
                    foreach (string file in files)
                    {
                        isolatedStorage.DeleteFile(file);
                        //System.Diagnostics.Debug.WriteLine("Temp file deleted: " + file);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Saving picture to camera roll failed: " + ex.HResult.ToString("x8") + " - " + ex.Message);
            }

            NavigationService.GoBack();
        }
    }
}