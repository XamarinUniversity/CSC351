using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Xamarin.Forms;
using FractlExplorer.Utility;
using FractlExplorer.UWP;

[assembly: Dependency(typeof(ImageCreator))]

namespace FractlExplorer.UWP
{
    public class ImageCreator : IImageCreator
    {
        public double ScaleFactor {
            get
            {
                return DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            }
        }

        public Task<Stream> CreateAsync(int[] data, int width, int height)
        {
            return Task.Run(async () =>
            {
                var pixels = new byte[data.Length * sizeof(int)];
                System.Buffer.BlockCopy(data, 0, pixels, 0, pixels.Length);

                InMemoryRandomAccessStream mras = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(
                    BitmapEncoder.PngEncoderId, mras);

                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore,
                                        (uint)width, (uint)height, 96, 96, pixels);
                await encoder.FlushAsync();
                mras.Seek(0);

                return mras.AsStreamForRead();
            });

        }
    }
}

