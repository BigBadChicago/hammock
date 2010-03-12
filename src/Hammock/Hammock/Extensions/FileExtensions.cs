#if !Silverlight
using System.Drawing.Imaging;
#endif

namespace Hammock.Extensions
{
    internal static class FileExtensions
    {
        public static int Megabytes(this int value)
        {
            return value*1024*1024;
        }

        public static int Kilobytes(this int value)
        {
            return value*1024;
        }

        public static int Bytes(this int value)
        {
            return value;
        }

#if !SILVERLIGHT
        public static string ToContentType(this ImageFormat format)
        {
            if(format == ImageFormat.Jpeg
#if !Smartphone
               || format.Guid == "{b96b3cae-0728-11d3-9d7b-0000f81ef32e}".AsGuid()
#endif
                )
            {
                return "image/jpeg";
            }

            if (format == ImageFormat.Gif 
#if !Smartphone
                || format.Guid == "{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}".AsGuid()
#endif
                )
            {
                return "image/gif";
            }

            if (format == ImageFormat.Png 
#if !Smartphone
                || format.Guid == "{b96b3caf-0728-11d3-9d7b-0000f81ef32e}".AsGuid()
#endif
                )
            {
                return "image/png";
            }

            if (format == ImageFormat.Bmp 
#if !Smartphone
                || format.Guid == "{b96b3cab-0728-11d3-9d7b-0000f81ef32e}".AsGuid() 
                || format.Guid == "{b96b3caa-0728-11d3-9d7b-0000f81ef32e}".AsGuid()
#endif
                )
            {
                return "image/bmp";
            }

#if !Smartphone
            return format == ImageFormat.Tiff ? "image/tiff" : "application/octet-stream";
#else
            return "application/octet-stream";
#endif
        }
#endif
    }
}