using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace ElectroTech.Services
{
    public abstract class MailService
    {
        private SmtpClient SmtpClient;
        protected string SenderMail { get; set; }
        protected string Password { get; set; }
        protected string Host { get; set; }
        protected int Port { get; set; }
        protected bool ssl { get; set; }

        protected void initializeSmtpClient()
        {
            SmtpClient = new SmtpClient();
            SmtpClient.Credentials = new NetworkCredential(SenderMail, Password);
            SmtpClient.Host = Host;
            SmtpClient.Port = Port;
            SmtpClient.EnableSsl = ssl;
        }

        public void SendMail(String subject, String body, List<string> recipientMail)
        {
            var mailMessage = new MailMessage();
            try
            {
                mailMessage.From = new MailAddress(SenderMail);
                foreach(string mail in recipientMail)
                {
                    mailMessage.To.Add(mail);
                }
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.Priority = MailPriority.Normal;
                SmtpClient.Send(mailMessage);

            }
            catch (Exception ex)
            {
                throw new Exception("Error al enviar el correo electrónico.", ex);
            }
            finally
            {
                mailMessage.Dispose();
                SmtpClient.Dispose();
            }
        }


    }
}
