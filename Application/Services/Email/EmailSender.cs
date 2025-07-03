using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Application.Utilities.Contractors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Email
{
    public sealed class EmailSender : INotificationSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration) {
            _configuration = configuration;
        }
        public async Task SendAsync(string To, string Title, string Body, Dictionary<string, string> Data = null)
        {
            SmtpClient smtpClient = new SmtpClient
            {
                Port = int.Parse(_configuration["MailSettings:Port"]),
                Host = _configuration["MailSettings:Host"],
                EnableSsl = false,
                //UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_configuration["MailSettings:Mail"], _configuration["MailSettings:Password"])
            };

            string senderEmail = _configuration["MailSettings:Mail"];
            string senderDisplayName = _configuration["MailSettings:DisplayName"];

            MailAddress senderAddress = new MailAddress(senderEmail, senderDisplayName);
            MailAddress toAddress = new MailAddress(To);
            MailMessage mailMessage = new MailMessage(senderAddress, toAddress)
            {
                Subject = Title,
                Body = Body,
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}