using System;
using System.Collections.Generic;
using System.Text;

namespace CameraExplorer.Models
{
    class CaptureResolution
    {
        public uint width;
        public uint height;

        public CaptureResolution(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return width + " x " + height;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(CaptureResolution))
            {
                var r = obj as CaptureResolution;
                return (r.width == width && r.height == height);
            }

            return base.Equals(obj);
        }

    }
}
