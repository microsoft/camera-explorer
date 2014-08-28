using CameraExplorer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.FileProperties;

namespace CameraExplorer.Utilities
{
    class CameraUtilities
    {

        public static IMediaEncodingProperties GetCurrentCaptureResolution(MediaCapture mediaCapture)
        {
            return mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.Photo);
        }

        public static IReadOnlyList<IMediaEncodingProperties> GetAvailableCaptureResolutions(MediaCapture mediaCapture)
        {
            return mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo);
            /*
            List<CaptureResolution> captureResolutions = new List<CaptureResolution>();

            if (mediaCapture.VideoDeviceController != null)
            {
                foreach (IMediaEncodingProperties encodingProperties in
                    mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo))
                {
                    try
                    {

                        var imageEncodingProperties = encodingProperties as ImageEncodingProperties;

                        if (imageEncodingProperties != null)
                        {
                            CaptureResolution resolution = new CaptureResolution(imageEncodingProperties.Width, imageEncodingProperties.Height);
                            captureResolutions.Add(resolution);
                        }

                        var videoEncodingProperties = encodingProperties as VideoEncodingProperties;

                        if (videoEncodingProperties != null)
                        {
                            CaptureResolution resolution = new CaptureResolution(videoEncodingProperties.Width, videoEncodingProperties.Height);
                            captureResolutions.Add(resolution);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to resolve available resolutions: " + ex.Message);
                    }
                }
            }

            return captureResolutions;*/
        }

        public static PhotoOrientation GetExifOrientationForDeviceOrientation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.NotRotated:
                    return PhotoOrientation.Rotate270;
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return PhotoOrientation.Normal;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return PhotoOrientation.FlipVertical;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return PhotoOrientation.Rotate90;

                default:
                    return PhotoOrientation.Normal;
            }
        }
    }
}
