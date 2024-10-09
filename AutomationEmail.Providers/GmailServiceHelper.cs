using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;

namespace AutomationEmail.Providers
{
    public class GmailServiceHelper
    {
        static string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.GmailReadonly };
        private GmailService _serivce;

        public GmailServiceHelper(string crendentialPath, string tokenPath)
        {
            using var stream = new FileStream(crendentialPath, FileMode.Open, FileAccess.Read);
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true)).Result;

            if(credential.Token.IsExpired(credential.Flow.Clock))
            {
                credential.RefreshTokenAsync(CancellationToken.None);
            }

            _serivce = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API .NET"
            });
        }

        public void ReadEmails()
        {
            var request = _serivce.Users.Messages.List("me");
            request.LabelIds = "INBOX";
            request.IncludeSpamTrash = false;

            var response = request.Execute();

            if(response != null && response.Messages != null)
            {
                foreach (var messageItem in response.Messages)
                {
                    var message = _serivce.Users.Messages.Get("me", messageItem.Id).Execute();
                    var subjectHeader = message.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");
                    Console.WriteLine($"Subject: {subjectHeader?.Value}");
                }
            }
        }


        public void SendEmail(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Brabo Dev", "brabo@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder()
            {
                TextBody = body
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var memoryStream = new MemoryStream();
            emailMessage.WriteTo(memoryStream);

            var rawMessage = Convert.ToBase64String(memoryStream.ToArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var message = new Message
            {
                Raw = rawMessage,
            };

            _serivce.Users.Messages.Send(message, "me").Execute();
        }
    }
}
