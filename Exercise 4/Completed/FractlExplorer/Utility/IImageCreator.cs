using System;
using System.IO;
using System.Threading.Tasks;

namespace FractlExplorer.Utility
{
    public interface IImageCreator
    {
        double ScaleFactor { get; }
        Task<Stream> CreateAsync(int[] data, int width, int height);
    }
}

