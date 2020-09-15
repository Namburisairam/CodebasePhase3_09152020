//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EventManagement.DataAccess.DataBase.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Notification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Notification()
        {
            this.UserNotifications = new HashSet<UserNotification>();
        }
    
        public int ID { get; set; }
        public int EventID { get; set; }
        public string Text { get; set; }
        public Nullable<System.DateTime> AddedON { get; set; }
        public Nullable<int> AddedBY { get; set; }
        public bool Status { get; set; }
        public Nullable<System.DateTime> ReadDate { get; set; }
        public string description { get; set; }
    
        public virtual Event Event { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserNotification> UserNotifications { get; set; }
    }
}