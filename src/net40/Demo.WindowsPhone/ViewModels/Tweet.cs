using Demo.WindowsPhone.Models;

namespace Demo.WindowsPhone.ViewModels
{
    public class Tweet
    {
        private readonly TwitterStatus _status;

        public Tweet(TwitterStatus status)
        {
            _status = status;
        }

        public string Text
        {
            get
            {
                return _status.Text;
            }
        }

        public string ScreenName
        {
            get
            {
                return _status.User.ScreenName;
            }
        }

        public string ProfileImageUrl
        {
            get
            {
                return _status.User.ProfileImageUrl;
            }
        }
    }
}
