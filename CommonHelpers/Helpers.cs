using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CommonHelpers
{
    public static class Helpers
    {
        private const string DefaultTimeFormat = @"hh:mm tt";
        private const string DefaultTimeFormatDisplay = @"hh:mmtt";
        private const string DefaultDateFormat = @"dd/MM/yyyy";
        private const string BcrypSalt = "terp";
        private static readonly ILog _logger =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Hash(string value)
        {
            return BCrypt.Net.BCrypt.HashPassword(value, BcrypSalt);
        }

        public static bool ValidateHash(string value, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(value, hash);
        }

        public static void LogInfo(string message)
        {
            _logger.Info(message);
        }
        public static string FirstCharToUpper(string input)
        {
            if (!String.IsNullOrEmpty(input))
                return input.First().ToString().ToUpper() + input.Substring(1);
            else
                return input;
        }
        public static void LogError(string message, Exception ex = null)
        {
            if (ex != null)
                _logger.Error(message, ex);
            else
                _logger.Error(message);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task SendEmail(string subject, string message, List<MemoryStream> stream = null, FileStream DocumentFile = null, MemoryStream textfile = null, bool isBodyHtml = false)
        {
            try
            {
                MailMessage mail = new MailMessage();
                string fromAddress = WebConfigurationManager.AppSettings["fromAddress"];
                string toAddress = WebConfigurationManager.AppSettings["toAddress"];
                string emailHost = WebConfigurationManager.AppSettings["emailHost"];
                string userID = WebConfigurationManager.AppSettings["userID"];
                string pwd = WebConfigurationManager.AppSettings["pwd"];
                string eSubj = subject;
                string eBody = message;
                int port = 587;

                if (stream != null)
                {
                    foreach (var st in stream)
                    {
                        mail.Attachments.Add(new Attachment(st, "Note.txt"));
                    }
                }

                if (DocumentFile != null)
                {
                    mail.Attachments.Add(new Attachment(DocumentFile, "Document.docx"));
                }
                if (textfile != null)
                {
                    mail.Attachments.Add(new Attachment(textfile, "Text.txt"));
                }

                SmtpClient SmtpServer = new SmtpClient(emailHost);
                mail.From = new MailAddress(fromAddress);
                mail.To.Add(toAddress);
                mail.Subject = eSubj;
                mail.Body = eBody;
                mail.IsBodyHtml = true;
                SmtpServer.Port = port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(userID, pwd);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Task SendMailExAsync(
       this System.Net.Mail.SmtpClient @this,
       System.Net.Mail.MailMessage message,
       CancellationToken token = default(CancellationToken))
        {
            // use Task.Run to negate SynchronizationContext
            return Task.Run(() => SendMailExImplAsync(@this, message, token));
        }

        private static async Task SendMailExImplAsync(
            System.Net.Mail.SmtpClient client,
            System.Net.Mail.MailMessage message,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();
            System.Net.Mail.SendCompletedEventHandler handler = null;
            Action unsubscribe = () => client.SendCompleted -= handler;

            handler = async (s, e) =>
            {
                unsubscribe();

                // a hack to complete the handler asynchronously
                await Task.Yield();

                if (e.UserState != tcs)
                    tcs.TrySetException(new InvalidOperationException("Unexpected UserState"));
                else if (e.Cancelled)
                    tcs.TrySetCanceled();
                else if (e.Error != null)
                    tcs.TrySetException(e.Error);
                else
                    tcs.TrySetResult(true);
            };

            client.SendCompleted += handler;
            try
            {
                client.SendAsync(message, tcs);
                using (token.Register(() => client.SendAsyncCancel(), useSynchronizationContext: false))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                unsubscribe();
            }
        }

        private static double DegToRad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double RadToDeg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static double GetDistance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(DegToRad(lat1)) * Math.Sin(DegToRad(lat2)) + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Cos(DegToRad(theta));
            dist = Math.Acos(dist);
            dist = RadToDeg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        public static string GetJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T ParseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetAppSetting(string settingKey, bool removetilt = false)
        {
            if (removetilt)
                return ConfigurationManager.AppSettings[settingKey].Replace("~/", "");
            else
                return ConfigurationManager.AppSettings[settingKey];
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static int ToSafeInteger(this string value)
        {
            int temp;
            int.TryParse(value, out temp);
            return temp;
        }
        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
        public static DateTime ToSafeDate(this string date)
        {
            DateTime rDate = new DateTime();
            DateTime.TryParse(date, out rDate);
            if (rDate.Year > 2000)
            {
                return rDate;
            }
            return DateTime.Now;
        }
        public static string GetUKDateFormat(DateTime dt)
        {
            return dt.ToString(DefaultDateFormat);
        }
        public static string GetUKDateTimeFormat(DateTime dt)
        {
            return string.Concat(dt.ToString(DefaultDateFormat), " ", dt.ToShortTimeString());
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static TimeSpan ParseTime(string time)
        {
            return DateTime.ParseExact(time, DefaultTimeFormat, CultureInfo.InvariantCulture).TimeOfDay;
        }
        public static string GetFormattedTime(string time)
        {

            var tsTime = TimeSpan.Parse(time);
            DateTime dt = DateTime.Today.Add(tsTime);
            string displayTime = dt.ToString(DefaultTimeFormatDisplay);
            return displayTime.ToLower();
        }

        public static string ImageToBase64(string imagePath)
        {
            var PhysicalPath = HostingEnvironment.MapPath(imagePath);
            if (!File.Exists(PhysicalPath))
            {
                return null;
            }
            FileStream fS = new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read);
            byte[] byt = new byte[fS.Length];
            fS.Read(byt, 0, (int)fS.Length);
            fS.Close();
            return Convert.ToBase64String(byt);
        }

        public static Stream FileToStream(string imagePath)
        {
            var PhysicalPath = HostingEnvironment.MapPath(imagePath);
            if (!File.Exists(PhysicalPath))
            {
                return null;
            }
            return new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read);
        }

        public static string ImageToBase64(Stream imageStream)
        {
            try
            {
                if (imageStream == null)
                {
                    return null;
                }
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    ms.Seek(0, System.IO.SeekOrigin.Begin);
                //    imageStream.CopyTo(ms);
                //    return Convert.ToBase64String(ms.ToArray());
                //}
                byte[] buffer = new byte[imageStream.Length];
                imageStream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string HtmlToString(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static Stream ResizeImage(Stream originalImageStream, double minDimesion)
        {
            if (originalImageStream == null)
            {
                return null;
            }
            var image = Image.FromStream(originalImageStream);
            float referenceDim = image.Width > image.Height ? image.Width : image.Height;
            var factor = referenceDim > minDimesion ? minDimesion / referenceDim : 1;
            var newWidth = (int)(image.Width * factor);
            var newHeight = (int)(image.Height * factor);
            var thumbnailBitmap = new Bitmap(newWidth, newHeight);

            var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
            thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
            thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
            thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
            thumbnailGraph.DrawImage(image, imageRectangle);
            var resizedImage = new MemoryStream();
            resizedImage.Seek(0, System.IO.SeekOrigin.Begin);
            thumbnailBitmap.Save(resizedImage, image.RawFormat);
            //thumbnailBitmap.Save(@"D:\Sample images\test.jpg", image.RawFormat);
            thumbnailGraph.Dispose();
            thumbnailBitmap.Dispose();
            image.Dispose();
            return resizedImage;
        }
    }
}
