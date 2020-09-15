using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonHelpers;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;
using QRCoder;

namespace EventManagement.BusinessLogic.Business
{
    public class QRCodeLogic : BaseLogic
    {
        public async Task<object> RecordQRCodeScanHistory(int attendeeId, int qrId)
        {
            Db.QRHistories.Add(new QRHistory
            {
                AttendesID = attendeeId,
                QRid = qrId,
                CommentON = DateTime.Now
            });
            await Db.SaveChangesAsync();
            return new { Message = "Success" };
        }

        public async Task<object> GetAttendeScanHistory(int attendeeId)
        {
            return await Db.QRHistories.Where(x => x.AttendesID == attendeeId)
                    .Select(x => new
                    {
                        x.CommentON,
                        x.QRCode.WebURL,
                    }).ToListAsync();
        }

        public string GenerateAttendeeQRCode(string imageSavePath, int attendeeID, int cmsUserID)
        {
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("{2}:{0},QRId:{3}", attendeeID, enQRCodeFor.Attendee, qrId), QRCodeGenerator.ECCLevel.Q);
            //QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            //var qrPath = saveQRCode(qrCode, imageSavePath);

            var qRCodeObj = new DataAccess.DataBase.Model.QRCode()
            {
                GeneratedBY = cmsUserID,
                GeneratedON = DateTime.Now,
                Status = true,
                Description = string.Empty,
                WebURL = string.Empty,
                AttendeeID = attendeeID
            };

            Db.QRCodes.Add(qRCodeObj);
            Db.SaveChanges();
            var qRPath = GenerateQRCode(string.Format("{1}:{0},QRId:{2}", attendeeID, enQRCodeFor.Attendee.ToString(), qRCodeObj.ID), imageSavePath);
            qRCodeObj.CodePath = qRPath;
            Db.SaveChanges();
            return qRPath;
        }

        public string GenerateExhibitorQRCode(string imageSavePath, int exhibitorID, int cmsUserID, string webURL)
        {
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode($"{exhibitorID}", QRCodeGenerator.ECCLevel.Q);
            //QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            //var qrPath = saveQRCode(qrCode, imageSavePath);


            var qRCodeObj = new DataAccess.DataBase.Model.QRCode()
            {
                GeneratedBY = cmsUserID,
                GeneratedON = DateTime.Now,
                Status = true,
                Description = string.Empty,
                WebURL = webURL ?? string.Empty,
                ExhibitorID = exhibitorID
            };

            Db.QRCodes.Add(qRCodeObj);
            Db.SaveChanges();
            var qRPath = GenerateQRCode(string.Format("{2}:{0},URL:{1},QRId:{3}", exhibitorID, webURL, enQRCodeFor.Exhibitor.ToString(), qRCodeObj.ID), imageSavePath);
            qRCodeObj.CodePath = qRPath;
            Db.SaveChanges();
            return qRPath;
        }

        public string GenerateVendorQRCode(string imageSavePath, int vendorID, int cmsUserID, string webURL)
        {
            try
            {
                var qRCodeObj = new DataAccess.DataBase.Model.QRCode()
                {
                    GeneratedBY = cmsUserID,
                    GeneratedON = DateTime.Now,
                    Status = true,
                    Description = string.Empty,
                    WebURL = webURL ?? string.Empty,
                    VendorID = vendorID
                };

                Db.QRCodes.Add(qRCodeObj);
                //Db.SaveChanges();
                var qRPath = GenerateQRCode(string.Format("{2}:{0},URL:{1},QRId:{3}", vendorID, webURL, enQRCodeFor.Vendor.ToString(), qRCodeObj.ID), imageSavePath);
                qRCodeObj.CodePath = qRPath;
                Db.SaveChanges();
                return qRPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string saveQRCode(QRCoder.QRCode qrCode, string imageSavePath)
        {
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var fileName = Guid.NewGuid().ToString() + ".png";
            qrCodeImage.Save(Path.Combine(imageSavePath, fileName));
            return fileName;
        }

        public string GenerateQRCode(string qRData, string imageSavePath)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qRData, QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            return saveQRCode(qrCode, imageSavePath);
        }
    }
}
