#region License

// TweetSharp
// Copyright (c) 2010 Daniel Crenna and Jason Diller
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace System.Net
{
    partial class WebClient
    {
        public void CancelAsync()
        {
            lock (this)
            {
                if (async_thread == null)
                    return;

                //
                // We first flag things as done, in case the Interrupt hangs
                // or the thread decides to hang in some other way inside the
                // event handlers, or if we are stuck somewhere else.  This
                // ensures that the WebClient object is reusable immediately
                //
                CompleteAsync();
                // t.Interrupt ();
            }
        }

        public void DownloadDataAsync(Uri address)
        {
            DownloadDataAsync(address, null);
        }

        private void CompleteAsync()
        {
            lock (this)
            {
                IsBusy = false;
                async_thread = null;
            }
        }

        public void DownloadDataAsync(Uri address, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            lock (this)
            {
                SetBusy();

                async = true;
                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var data = DownloadDataCore(address, userToken);
                                                      OnDownloadDataCompleted(
                                                                                 new DownloadDataCompletedEventArgs(
                                                                                     data, null, false, userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnDownloadDataCompleted(
                                                                                 new DownloadDataCompletedEventArgs(
                                                                                     null, null, true, userToken));
                                                      throw;
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnDownloadDataCompleted(
                                                                                 new DownloadDataCompletedEventArgs(
                                                                                     null, e, false, userToken));
                                                  }
                                              });

                async_thread.Start();
            }
        }

        public void DownloadFileAsync(Uri address, string fileName)
        {
            DownloadFileAsync(address, fileName, null);
        }

        public void DownloadFileAsync(Uri address, string fileName, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      DownloadFileCore(address, fileName, userToken);
                                                      OnDownloadFileCompleted(
                                                                                 new AsyncCompletedEventArgs(null, false,
                                                                                                             userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnDownloadFileCompleted(
                                                                                 new AsyncCompletedEventArgs(null, true,
                                                                                                             userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnDownloadFileCompleted(
                                                                                 new AsyncCompletedEventArgs(e, false,
                                                                                                             userToken));
                                                  }
                                              });
                async_thread.Start();
            }
        }

        public void DownloadStringAsync(Uri address)
        {
            DownloadStringAsync(address, null);
        }

        public void DownloadStringAsync(Uri address, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var bytes = DownloadDataCore(address,
                                                                                   userToken);
                                                      var data = encoding.GetString(bytes, 0, bytes.Length);
                                                      OnDownloadStringCompleted(
                                                                                   new DownloadStringCompletedEventArgs(
                                                                                       data, null, false, userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnDownloadStringCompleted(
                                                                                   new DownloadStringCompletedEventArgs(
                                                                                       null, null, true, userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnDownloadStringCompleted(
                                                                                   new DownloadStringCompletedEventArgs(
                                                                                       null, e, false, userToken));
                                                  }
                                              });
                async_thread.Start();
            }
        }

        public void OpenReadAsync(Uri address)
        {
            OpenReadAsync(address, null);
        }

        public void OpenReadAsync(Uri address, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  WebRequest request = null;
                                                  try
                                                  {
                                                      request = SetupRequest(address);
                                                      var response = request.GetResponse();
                                                      var stream = ProcessResponse(response);
                                                      OnOpenReadCompleted(
                                                                             new OpenReadCompletedEventArgs(stream, null,
                                                                                                            false,
                                                                                                            userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      if (request != null)
                                                          request.Abort();

                                                      OnOpenReadCompleted(new OpenReadCompletedEventArgs(null,
                                                                                                         null,
                                                                                                         true,
                                                                                                         userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnOpenReadCompleted(new OpenReadCompletedEventArgs(null,
                                                                                                         e,
                                                                                                         false,
                                                                                                         userToken));
                                                  }
                                              });

                async_thread.Start();
            }
        }

        public void OpenWriteAsync(Uri address)
        {
            OpenWriteAsync(address, null);
        }

        public void OpenWriteAsync(Uri address, string method)
        {
            OpenWriteAsync(address, method, null);
        }

        public void OpenWriteAsync(Uri address, string method, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  WebRequest request = null;
                                                  try
                                                  {
                                                      request = SetupRequest(address, method, true);
                                                      var stream = request.GetRequestStream();
                                                      OnOpenWriteCompleted(
                                                                              new OpenWriteCompletedEventArgs(stream,
                                                                                                              null,
                                                                                                              false,
                                                                                                              userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      if (request != null)
                                                          request.Abort();
                                                      OnOpenWriteCompleted(
                                                                              new OpenWriteCompletedEventArgs(null, null,
                                                                                                              true,
                                                                                                              userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnOpenWriteCompleted(
                                                                              new OpenWriteCompletedEventArgs(null, e,
                                                                                                              false,
                                                                                                              userToken));
                                                  }
                                              });
                async_thread.Start();
            }
        }

        public void UploadDataAsync(Uri address, byte[] data)
        {
            UploadDataAsync(address, null, data);
        }

        public void UploadDataAsync(Uri address, string method, byte[] data)
        {
            UploadDataAsync(address, method, data, null);
        }

        public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var data2 = UploadDataCore(address, method, data, userToken);

                                                      OnUploadDataCompleted(
                                                                               new UploadDataCompletedEventArgs(data2,
                                                                                                                null,
                                                                                                                false,
                                                                                                                userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnUploadDataCompleted(
                                                                               new UploadDataCompletedEventArgs(null,
                                                                                                                null,
                                                                                                                true,
                                                                                                                userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnUploadDataCompleted(
                                                                               new UploadDataCompletedEventArgs(null, e,
                                                                                                                false,
                                                                                                                userToken));
                                                  }
                                              });
                async_thread.Start();
            }
        }

        public void UploadFileAsync(Uri address, string fileName)
        {
            UploadFileAsync(address, null, fileName);
        }

        public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var data = UploadFileCore(address, method, fileName, userToken);
                                                      OnUploadFileCompleted(
                                                                               new UploadFileCompletedEventArgs(data,
                                                                                                                null,
                                                                                                                false,
                                                                                                                userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnUploadFileCompleted(
                                                                               new UploadFileCompletedEventArgs(null,
                                                                                                                null,
                                                                                                                true,
                                                                                                                userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnUploadFileCompleted(
                                                                               new UploadFileCompletedEventArgs(null, e,
                                                                                                                false,
                                                                                                                userToken));
                                                  }
                                              });

                async_thread.Start();
            }
        }

        public void UploadStringAsync(Uri address, string data)
        {
            UploadStringAsync(address, null, data);
        }

        public void UploadStringAsync(Uri address, string method, string data)
        {
            UploadStringAsync(address, method, data, null);
        }

        public void UploadStringAsync(Uri address, string method, string data, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            lock (this)
            {
                SetBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var data2 = UploadString(address, method, data);
                                                      OnUploadStringCompleted(
                                                                                 new UploadStringCompletedEventArgs(
                                                                                     data2, null, false, userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnUploadStringCompleted(
                                                                                 new UploadStringCompletedEventArgs(
                                                                                     null, null, true, userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnUploadStringCompleted(
                                                                                 new UploadStringCompletedEventArgs(
                                                                                     null, e, false, userToken));
                                                  }
                                              });
                async_thread.Start();
            }
        }

        public void UploadValuesAsync(Uri address, NameValueCollection values)
        {
            UploadValuesAsync(address, null, values);
        }

        public void UploadValuesAsync(Uri address, string method, NameValueCollection values)
        {
            UploadValuesAsync(address, method, values, null);
        }

        public void UploadValuesAsync(Uri address, string method, NameValueCollection values, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (values == null)
                throw new ArgumentNullException("values");

            lock (this)
            {
                CheckBusy();
                async = true;

                async_thread = new Thread(delegate
                                              {
                                                  try
                                                  {
                                                      var data = UploadValuesCore(address, method, values, userToken);
                                                      OnUploadValuesCompleted(
                                                                                 new UploadValuesCompletedEventArgs(
                                                                                     data, null, false, userToken));
                                                  }
                                                  catch (ThreadInterruptedException)
                                                  {
                                                      OnUploadValuesCompleted(
                                                                                 new UploadValuesCompletedEventArgs(
                                                                                     null, null, true, userToken));
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      OnUploadValuesCompleted(
                                                                                 new UploadValuesCompletedEventArgs(
                                                                                     null, e, false, userToken));
                                                  }
                                              });

                async_thread.Start();
            }
        }
    }
}