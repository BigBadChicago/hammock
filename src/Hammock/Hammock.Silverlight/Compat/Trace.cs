using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace Hammock.Silverlight.Compat
{
    public class Trace : IDisposable
    {
        private static readonly IsolatedStorageFile _file;
        private static IsolatedStorageFileStream _fileStream;
        private static StreamWriter _streamWriter;

        static Trace()
        {
            _file = IsolatedStorageFile.GetUserStoreForApplication();
            _fileStream = _file.OpenFile("TweetSharp.trace",
                                         FileMode.OpenOrCreate, 
                                         FileAccess.ReadWrite, 
                                         FileShare.ReadWrite);

            _streamWriter = new StreamWriter(_fileStream)
                                {
                                    AutoFlush = true
                                };
        }

        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Flush();
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }

            if(_streamWriter != null)
            {
                _streamWriter.Close();
                _streamWriter.Dispose();
                _streamWriter = null;
            }
        }

        public static void WriteLine(string message)
        {
            _streamWriter.WriteLine(message);
        }

        public static void WriteLine(string message, params object[] args)
        {
            _streamWriter.WriteLine(message, args);
        }

        public static void WriteLine(StringBuilder sb)
        {
            _streamWriter.WriteLine(sb.ToString());
        }
    }
}


