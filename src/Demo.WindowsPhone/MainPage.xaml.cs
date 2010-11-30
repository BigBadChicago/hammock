using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Demo.WindowsPhone.Models;
using Demo.WindowsPhone.Serialization;
using Demo.WindowsPhone.ViewModels;
using Hammock;
using Hammock.Silverlight.Compat;
using Hammock.Web;

namespace Demo.WindowsPhone
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPageLoaded;
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadPublicTweets();

            RestClient client = new RestClient
            {
                Authority = "http://www.tumblr.com/api"
            };

            RestRequest request = new RestRequest
            {
                Path = "/write",
                Method = WebMethod.Post
            };

            const string data = "This is a test stream";
            var capturedImage = new MemoryStream(Encoding.UTF8.GetBytes(data));

            request.AddParameter("email", "email");
            request.AddParameter("password", "password");
            request.AddParameter("generator", "YABA - Windows Phone 7");
            request.AddParameter("type", "photo");
            request.AddParameter("caption", "caption");
            request.AddFile("data", "image1.jpg", capturedImage, "image/jpeg");
            client.BeginRequest(request);
        }

        private void LoadPublicTweets()
        {
            var dispatcher = Deployment.Current.Dispatcher;

            var client = new RestClient
                             {
                                 Authority = "https://api.twitter.com",
                                 VersionPath = "1",
                                 DecompressionMethods = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                                 Deserializer = new JsonSerializer()
                             };

            var request = new RestRequest
                              {
                                  Path = "statuses/public_timeline.json"
                              };

            client.BeginRequest<IEnumerable<TwitterStatus>>(request,
                                                            (req, response, state) =>
                                                                {
                                                                    var statuses = response.ContentEntity;
                                                                    foreach (var tweet in statuses.Select(s => new Tweet(s)))
                                                                    {
                                                                        var inline = tweet;
                                                                        dispatcher.BeginInvoke(() => tweets.Items.Add(inline));
                                                                    }
                                                                }
                );
        }
    }
}