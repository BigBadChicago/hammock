using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;

namespace OAuthOutOfBrowser.Controls
{
    public partial class Login
    {
        public string Result { get; set; }
        public Uri Uri
        {
            get
            {
                return this.WebBrowser.Source;
            }
            set
            {
                this.WebBrowser.Source = value;
            }
        }

        public Login()
        {

            InitializeComponent();

            this.WebBrowser.ScriptNotify += WebBrowserScriptNotify;
        }

        void WebBrowserScriptNotify(object sender, NotifyEventArgs e)
        {
            string url = HttpUtility.UrlDecode(e.Value);
            string sessionString = HttpUtility.UrlDecode(new Uri(url).Query);
            Result = url;
            
            DialogResult = true;
        }
    }
}

