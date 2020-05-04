using System;

using Windows.UI.Xaml.Media.Imaging;

namespace App1.Client.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage ImageFromAssetsFile(string fileName)
        {
            var image = new BitmapImage(new Uri($"ms-appx:///Assets/{fileName}"));
            return image;
        }
    }
}
