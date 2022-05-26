using Hangfire;
using Logic.IHelper;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Logic.Services
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
        void SendEmail(string toEmail, string subject, string message);
        void CallHangfires(EmailMessage emailMessage, string path);
        void SendWithAttachment(EmailMessage emailMessage, string path);
        void SendEmailWithBackedUpFile(string toEmail, string subject, string path);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public void SendEmail(string toEmail, string subject, string message)
        {
            EmailAddress fromAddress = new EmailAddress()
            {
                Name = "BiVi SOft Backup Team",
                Address = _emailConfiguration.SmtpUsername,
            };

            List<EmailAddress> fromAddressList = new List<EmailAddress>
            {
                        fromAddress
            };
            EmailAddress toAddress = new EmailAddress()
            {
                Name = "BiVi SOft Backup Team",
                Address = toEmail
            };
            List<EmailAddress> toAddressList = new List<EmailAddress>
            {
                    toAddress
            };

            EmailMessage emailMessage = new EmailMessage()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
                Content = message
            };
            CallHangfire(emailMessage);
        }

        public void CallHangfire(EmailMessage emailMessage)
        {
            BackgroundJob.Enqueue(() => Send(emailMessage));
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                // emailClient.Timeout = 30000;
                //emailClient.LocalDomain = "https://localhost:44300";
                //The last parameter here is to use SSL (Which you should!)
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
        }

        public void SendEmailWithBackedUpFile(string toEmail, string subject, string path)
        {
            EmailAddress fromAddress = new EmailAddress()
            {
                Name = "BiVi Soft Backup Team",
                Address = _emailConfiguration.SmtpUsername,
            };

            List<EmailAddress> fromAddressList = new List<EmailAddress>
            {
               fromAddress
            };

            EmailAddress toAddress = new EmailAddress()
            {
                Name = "BiVi Soft Backup Team",
                Address = toEmail
            };

            List<EmailAddress> toAddressList = new List<EmailAddress>
            {
               toAddress
            };

            EmailMessage emailMessage = new EmailMessage()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
            };
            CallHangfires(emailMessage, path);
        }

        public void CallHangfires(EmailMessage emailMessage, string path)
        {
            BackgroundJob.Enqueue(() => SendWithAttachment(emailMessage, path));
        }

        public void SendWithAttachment(EmailMessage emailMessage, string path)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Subject = emailMessage.Subject;

            var attachment = new MimePart("x-zip", "application/zip")
            {
                Content = new MimeContent(File.OpenRead(path)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(path)
            };

            var multipart = new Multipart("mixed");
            multipart.Add(attachment);
            message.Attachments.Append(attachment);
            message.Body = multipart;


            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
            {

                emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                // emailClient.Timeout = 30000;
                //emailClient.LocalDomain = "https://localhost:44300";
                //The last parameter here is to use SSL (Which you should!)
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");


                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
        }
    }
}
