using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Threading.Tasks;
using WabPApi.Models;

namespace WabPApi.Services
{
    public interface IMailService
    {
        //Task<string> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendMailAsync(MailData mailData);
    }

    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;
        }

        public async Task<bool> SendMailAsync(MailData mailData)
        {
            try
            {
                using (MimeMessage emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                    emailMessage.From.Add(emailFrom);
                    MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
                    emailMessage.To.Add(emailTo);

                    // you can add the CCs and BCCs here.
                    //emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "cc@example.com"));
                    emailMessage.Bcc.Add(new MailboxAddress("Wabp Ebooks", "timotech@yahoo.com"));

                    emailMessage.Subject = mailData.EmailSubject;

                    BodyBuilder emailBodyBuilder = new BodyBuilder();
                    emailBodyBuilder.TextBody = mailData.EmailBody;

                    emailMessage.Body = emailBodyBuilder.ToMessageBody();
                    //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                    using (SmtpClient mailClient = new SmtpClient())
                    {
                        //await mailClient.ConnectAsync(_mailSettings.Server, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                        await mailClient.ConnectAsync(_mailSettings.Server, _mailSettings.Port,false);
                        await mailClient.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
                        await mailClient.SendAsync(emailMessage);
                        await mailClient.DisconnectAsync(true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                return false;
            }
        }
    }

    //public class SendMailService : IMailService
    //{
    //    private SmtpClient _smtpClient;

    //    public SendMailService(SmtpClient smtpClient)
    //    {
    //        _smtpClient = smtpClient;
    //    }

    //    public async Task<string> SendEmailAsync(string toEmail, string subject, string body)
    //    {
    //        try
    //        {
    //            await _smtpClient.SendMailAsync(new MailMessage("info@wabpapp.com", toEmail, subject, body));
    //            return "Success";
    //        }
    //        catch (Exception ex)
    //        {
    //            return ex.Message;
    //        }
    //    }
    //}
}
