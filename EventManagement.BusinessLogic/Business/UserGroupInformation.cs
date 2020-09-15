using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Business
{
    public class UserGroupInformation
    {
        public int ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string ChannelImage { get; set; }

        public List<int> ChannelReceivers { get; set; }

    }
}
