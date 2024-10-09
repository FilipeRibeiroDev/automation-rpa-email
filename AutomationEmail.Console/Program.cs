using AutomationEmail.Providers;

namespace AutomationEmail.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var gmail = new GmailServiceHelper("client_secret.json", "token");
            gmail.SendEmail("Brabo@brabo.com.dev", "Teste do Brabo Dev", "Teste do Brabo Dev Chorei!");
            //gmail.ReadEmails();
        }
    }
}
