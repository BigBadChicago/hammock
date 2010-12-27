using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using Demo.WindowsPhone.Configuration;
using Demo.WindowsPhone.Models;
using Demo.WindowsPhone.Serialization;
using Demo.WindowsPhone.ViewModels;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Silverlight.Compat;
using Hammock.Web;
using Microsoft.Practices.Mobile.Configuration;

namespace Demo.WindowsPhone
{
    public partial class MainPage
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private readonly string _twitPicKey;
        
        public MainPage()
        {
            InitializeComponent();

            var section = (ApplicationSettingsSection)ConfigurationManager.GetSection("ApplicationSettings");
            _consumerKey = section.AppSettings["ConsumerKey"].Value;
            _consumerSecret = section.AppSettings["ConsumerSecret"].Value;
            _accessToken = section.AppSettings["AccessToken"].Value;
            _accessTokenSecret = section.AppSettings["AccessTokenSecret"].Value;
            _twitPicKey = section.AppSettings["TwitPicKey"].Value;

            PreloadResources();

            Loaded += MainPageLoaded;
        }
        
        private static void PreloadResources()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();
            if(store.FileExists("_failwhale.jpg"))
            {
                store.DeleteFile("_failwhale.jpg");
            }
            MoveToIsolatedStorage("_failwhale.jpg");
        }

        private static void MoveToIsolatedStorage(string path)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            if (store.GetFileNames(path).Length != 0)
            {
                return;
            }

            using (var target = new IsolatedStorageFileStream(path, FileMode.Create, store))
            {
                path = string.Format("Demo.WindowsPhone.{0}", path);

                using(var source = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                {
                    if (source != null)
                    {
                        var content = new byte[source.Length];
                        source.Read(content, 0, content.Length);
                        target.Write(content, 0, content.Length);
                    }
                }
            }
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadPublicTweets();

            var client = new RestClient
                             {
                                 Authority = "http://api.twitpic.com/",
                                 VersionPath = "2"
                             };
            
            // Prepare an OAuth Echo request to TwitPic
            var request = PrepareEchoRequest();
            request.Path = "uploadAndPost.xml";
            request.AddField("key", _twitPicKey); // <-- Sign up with TwitPic to get an API key
            request.AddField("message", "Failwhale!");
            request.AddFile("media", "failwhale", "_failwhale.jpg", "image/jpeg"); // <-- This overload uses IsolatedStorage
            
            client.BeginRequest(request, (req, resp, state) => Debug.Assert(resp.StatusCode == HttpStatusCode.OK));
        }

        public RestRequest PrepareEchoRequest()
        {
            var client = new RestClient
            {
                Authority = "https://api.twitter.com",
                VersionPath = "1",
                UserAgent = "TweetSharp"
            };

            var request = new RestRequest
            {
                Method = WebMethod.Get,
                Path = "account/verify_credentials.json",
                Credentials = OAuthCredentials.ForProtectedResource(
                    _consumerKey, _consumerSecret, _accessToken, _accessTokenSecret
                    )
            };

            return OAuthCredentials.DelegateWith(client, request);
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