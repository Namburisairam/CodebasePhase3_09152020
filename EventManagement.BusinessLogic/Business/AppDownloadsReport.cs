using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Business
{
    public class AppDownloadsReport
    {
        public int ID { get; set; }

        public string UserName { get; set; }

        public DateTime DownloadDate { get; set; }

        //public int AttendeeId { get; set; }
        public DateTime LastLoginDate { get; set; }

        public DateTime LastVisitDate { get; set; }

    }
}
