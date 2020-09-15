using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business.Email_Notifier
{
    public class EmailSender : EmailNotifier
    {
        Entities db = new Entities();

        public EmailSender(string toAddress, string subject) : base(toAddress)
        {
            FromAddress = db.Configs.FirstOrDefault()?.SupportEmail;
            this.Subject = subject;
        }

        // it takes default subject if subject is not provoded
        public EmailSender(string toAddress) : base(toAddress)
        {
            FromAddress = db.Configs.FirstOrDefault()?.SupportEmail;
        }

        public bool SendEmailWithAttachment(params Stream[] streams)
        {
            try
            {
                SendEmail(FromAddress, streams);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendResetPasswordEmail(int code)
        {
            try
            {
                eBody = $"Please use this code to reset the password <br/> Code: {code} ";
                Subject = "Reset password";
                SendEmail(FromAddress);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task SendEmailAsync(string body)
        {
            try
            {
                using (var EmailNotifier = new EmailNotifier(ToAddress))
                {
                    EmailNotifier.Subject = Subject;
                    EmailNotifier.eBody = body;
                    await EmailNotifier.SendEmailAsync(db.Configs.FirstOrDefault()?.SupportEmail);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public override void Dispose()
        //{
        //    db.Dispose();
        //    base.Dispose();
        //}

    }
}
