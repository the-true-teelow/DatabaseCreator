using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace DatabaseCreator
{
    class ImageOperations
    {
        /// <summary>
        /// Converts an image from a given path to base64 and returns the result as a string
        /// </summary>
        /// <param name="path">the image path</param>
        /// <returns>base64 encoded image data</returns>
        public static String ImageToBase64(String path)
        {
            if (path == null || path == "")
            {
                return null;
            }

            FileInfo info = new FileInfo(path);
            String newPath = path;
            using (Image image = Image.FromFile(path))
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                using (Bitmap resizedImage = ResizeImage(image, 864, 1152))
                {
                    newPath = info.DirectoryName + "\\"
                            + info.Name.Substring(0, info.Name.LastIndexOf(info.Extension))
                            + "_" + resizedImage.Width + "_" + resizedImage.Height
                            + info.Extension;
                    resizedImage.Save(newPath,
                        ImageFormat.Jpeg);
                }
            }

            return Convert.ToBase64String(File.ReadAllBytes(newPath));
        }


        /// <summary>
        /// Changes the size of an image to a user-given value 
        /// </summary>
        /// <param name="image">the image object, which shall be resized</param>
        /// <param name="width">the desired weight</param>
        /// <param name="height">the desired height</param>
        /// <returns>the resized image asa Bitmap</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
