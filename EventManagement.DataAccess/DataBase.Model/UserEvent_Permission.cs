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
    
    public partial class UserEvent_Permission
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int PermissionID { get; set; }
    
        public virtual AppUser AppUser { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
