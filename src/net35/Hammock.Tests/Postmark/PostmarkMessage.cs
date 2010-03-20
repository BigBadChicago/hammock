using System.Collections.Specialized;
using System.Net.Mail;
using System.Web;

namespace Hammock.Tests.Postmark
{
    public class PostmarkMessage
    {
        public PostmarkMessage()
        {
            Headers = new NameValueCollection(0);
        }

        public PostmarkMessage(string from, string to, string subject, string body) : this(from, to, subject, body, null)
        {

        }

        public PostmarkMessage(string from, string to, string subject, string body, NameValueCollection headers)
        {
            var isHtml = !body.Equals(HttpUtility.HtmlEncode(body));

            From = from;
            To = to;
            Subject = subject;
            TextBody = isHtml ? null : body;
            HtmlBody = isHtml ? body : null;
            Headers = headers ?? new NameValueCollection(0);
        }

        public PostmarkMessage(MailMessage message)
        {
            From = message.From.DisplayName;
            To = message.To.Count > 0 ? message.To[0].DisplayName : null;
            Subject = message.Subject;
            HtmlBody = message.IsBodyHtml ? message.Body : null;
            TextBody = message.IsBodyHtml ? null : message.Body;
            ReplyTo = message.ReplyTo.DisplayName;
            Headers = message.Headers;
        }

        public string From { get; set; }
        public string To { get; set; }
        public string ReplyTo { get; set; }
        public string Subject { get; set; }

        public string HtmlBody { get; set; }
        public string TextBody { get; set; }

        public NameValueCollection Headers { get; set; }
    }
}