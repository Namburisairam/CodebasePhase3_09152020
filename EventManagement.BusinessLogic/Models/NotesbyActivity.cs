﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Models
{
    public class NotesbyActivity
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Text { get; set; }
        public string ActivityName { get; set; }

    }
}
