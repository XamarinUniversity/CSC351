using Xamarin.Forms;
using FractlExplorer.Droid;
using Android.Graphics;
using System.IO;
using System.Threading.Tasks;
using Java.Nio;
using FractlExplorer.Utility;
using Android.Content.Res;

[assembly: Dependency(typeof(ImageCreator))]

namespace FractlExplorer.Droid
{
    public class ImageCreator : IImageCreator
    {
        public double ScaleFactor {
            get {
                return Resources.System.DisplayMetrics.Density;
            }
        }

        public Task<Stream> CreateAsync(int[] pixels, int width, int height)
        {
            return Task.Run<Stream>(() => {

                Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                bitmap.CopyPixelsFromBuffer(IntBuffer.Wrap(pixels));

                MemoryStream ms = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
                ms.Flush();
                ms.Position = 0;

                return ms;
            });
        }
    }
}

