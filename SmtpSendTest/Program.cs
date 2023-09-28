using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SmtpSendTest
{
    internal class Program
    {
        private class SmtpConfig
        {
            public string SmtpServer { get; set; }
            public int Port { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }
        private class NotificationsConfig
        {
            public string DefaultSender { get; set; }
            public SmtpConfig Smtp { get; set; }
        }

        private static SmtpClient CreateClient(SmtpConfig config)
        {
            return new SmtpClient(config.SmtpServer, config.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(config.Login, config.Password)
            };
        }

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("One parameter is required: ToEmail");
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);

            IConfiguration config = builder.Build();
            var notificationsConfig = config.GetSection("Notifications").Get<NotificationsConfig>();

            using var mailMsg = new MailMessage();

            mailMsg.From = new MailAddress(notificationsConfig.DefaultSender);
            mailMsg.To.Add(new MailAddress(args[0]));
            mailMsg.ReplyToList.Add(mailMsg.From);

            mailMsg.Subject = "Test";
            mailMsg.Body = "Test";
            mailMsg.IsBodyHtml = false;

            using var client = CreateClient(notificationsConfig.Smtp);

            await client.SendMailAsync(mailMsg);
        }
    }
}