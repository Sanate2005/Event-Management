using System.Net.Mail;
using System.Net;

namespace Final_back.Mails
{
    public class EmailSender
    {
        public void sendMail(string to, string subject, string content)
        {
            SmtpClient smtpClient =
            new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new
                NetworkCredential("putita.misha2005@gmail.com", "vwsm gyfq hmne nxqe");

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("putita.misha2005@gmail.com");
            mailMessage.To.Add(to);
            mailMessage.Subject = subject;
            mailMessage.Body = content;
            mailMessage.IsBodyHtml = true;

            smtpClient.Send(mailMessage);
        }
    }
}
