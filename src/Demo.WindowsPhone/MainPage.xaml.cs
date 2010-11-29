using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Demo.WindowsPhone.Models;
using Demo.WindowsPhone.Serialization;
using Demo.WindowsPhone.ViewModels;
using Hammock;
using Hammock.Silverlight.Compat;

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
                            Tweet inline = tweet;
                            dispatcher.BeginInvoke(() => tweets.Items.Add(inline));
                        }
                    }
                );

        }
    }
}