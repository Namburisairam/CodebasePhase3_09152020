using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using EventManagement.BusinessLogic.Interfaces;

namespace EventManagement.BusinessLogic.Business.Email_Notifier
{
    public class EmailNotifier : INotifier, IDisposable
    {
        protected string FromAddress { get; set; }

        protected string ToAddress { get; private set; }

        public string Subject { get; set; }

        public string AttchmentName { get; private set; }

        const int Port = 587;

        MailMessage Mail = new MailMessage();

        //Entities db = new Entities();

        public static string EmailHost
        {
            get
            {
                return WebConfigurationManager.AppSettings["emailHost"];
            }
        }

        public static string UserName
        {
            get
            {
                return WebConfigurationManager.AppSettings["userID"];
            }
        }

        public static string Password
        {
            get
            {
                return WebConfigurationManager.AppSettings["pwd"];
            }
        }

        public string eBody { get; set; }

        public EmailNotifier(string toAddress)
        {
            ToAddress = toAddress;
        }

        public EmailNotifier(string toAddress, string eBody) : this(toAddress)
        {
            this.eBody = eBody;
        }


        public virtual void SendEmail(string fromAddress, params Stream[] stream)
        {
            foreach (var st in stream)
            {
                Mail.Attachments.Add(new Attachment(st, st is FileStream ? (st as FileStream).Name : DateTime.Now.ToString() + ".txt"));
            }

            SmtpClient SmtpServer = new SmtpClient(EmailHost);
            Mail.From = new MailAddress(fromAddress, fromAddress);
            Mail.To.Add(ToAddress);
            Mail.Subject = Subject;
            Mail.Body = eBody;
            Mail.IsBodyHtml = true;
            SmtpServer.Port = Port;
            SmtpServer.Credentials = new System.Net.NetworkCredential(UserName, Password);
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(Mail);
        }

        /// <summary>
        /// ToDo: Should make code reusable
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="stream"></param>
        public virtual async Task SendEmailAsync(string fromAddress, params Stream[] stream)
        {
            foreach (var st in stream)
            {
                Mail.Attachments.Add(new Attachment(st, st is FileStream ? (st as FileStream).Name : DateTime.Now.ToString() + ".txt"));
            }

            SmtpClient SmtpServer = new SmtpClient(EmailHost);
            Mail.From = new MailAddress(fromAddress, fromAddress);
            Mail.To.Add(ToAddress);
            Mail.Subject = Subject;
            Mail.Body = eBody;
            Mail.IsBodyHtml = true;
            SmtpServer.Port = Port;
            SmtpServer.Credentials = new System.Net.NetworkCredential(UserName, Password);
            SmtpServer.EnableSsl = true;
            await SmtpServer.SendMailAsync(Mail);
        }

        public virtual void Dispose()
        {
            //db.Dispose();
            Mail.Dispose();
        }
    }
}
