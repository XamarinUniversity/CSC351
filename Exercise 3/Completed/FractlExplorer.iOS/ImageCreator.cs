using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Xamarin.Forms;
using UIKit;
using FractlExplorer.Utility;
using FractlExplorer.iOS;
using System.Runtime.InteropServices;

[assembly: Dependency(typeof(ImageCreator))]

namespace FractlExplorer.iOS
{
    public class ImageCreator : IImageCreator
    {
        GCHandle handle;
        
        public double ScaleFactor {
            get {
                return (double) UIScreen.MainScreen.Scale;
            }
        }

        public Task<Stream> CreateAsync(int[] data, int width, int height)
        {
            return Task.Run(() => {
                // Must pin the memory since we are passing it into a native method.
                this.handle = GCHandle.Alloc (data, GCHandleType.Pinned);
                IntPtr buff = this.handle.AddrOfPinnedObject ();
                var provider = new CGDataProvider (buff, width * height * 4, ptr => this.handle.Free ());

                const int bitsPerComponent = 8;
                const int bitsPerPixel = 32;
                int bytesPerRow = 4 * width;

                var image = new CGImage(width, height,
                    bitsPerComponent,
                    bitsPerPixel,
                    bytesPerRow,
                    CGColorSpace.CreateDeviceRGB(),
                    CGBitmapFlags.ByteOrder32Big, // ABGR
                    provider,
                    null, false, CGColorRenderingIntent.Default);

                return new UIImage(image).AsPNG().AsStream();

            });
        }
    }
}

