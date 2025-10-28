using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpotiffyWidget.Helpers
{
    public static class SaveImageSource
    {
        public static bool SaveImage(ImageSource imageSource, string filePath)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                try
                {
                    BitmapEncoder encoder;

                    // Uzantıya göre encoder seçelim
                    string ext = Path.GetExtension(filePath).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg")
                        encoder = new JpegBitmapEncoder();
                    else
                        encoder = new PngBitmapEncoder(); // default png

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("ImageSource bir BitmapSource değil!");
                return false;
            }
        }
    }
}
