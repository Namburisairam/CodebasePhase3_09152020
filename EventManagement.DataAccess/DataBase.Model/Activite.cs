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
    
    public partial class Activite
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Activite()
        {
            this.BookMarks = new HashSet<BookMark>();
            this.Notes = new HashSet<Note>();
            this.QRCodes = new HashSet<QRCode>();
            this.QRCodes1 = new HashSet<QRCode>();
            this.Documents = new HashSet<Document>();
            this.Surveys = new HashSet<Survey>();
            this.SurveySubmittedFors = new HashSet<SurveySubmittedFor>();
            this.ExhibitorActivities = new HashSet<ExhibitorActivity>();
            this.SponsorActivities = new HashSet<SponsorActivity>();
            this.VendorActivities = new HashSet<VendorActivity>();
            this.WaiverFormTemplateMappings = new HashSet<WaiverFormTemplateMapping>();
        }
    
        public int ID { get; set; }
        public Nullable<int> EventID { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public string Address { get; set; }
        public Nullable<System.DateTime> CreateON { get; set; }
        public bool Status { get; set; }
        public Nullable<int> ActivityTypeid { get; set; }
        public Nullable<int> GalacticActivityId { get; set; }
        public string Name { get; set; }
        public Nullable<double> latitude { get; set; }
        public Nullable<double> longitude { get; set; }
        public Nullable<int> FloorRegionMappingID { get; set; }
        public Nullable<int> FloorMapLocationID { get; set; }
        public bool IsScheduleNotificationSent { get; set; }
        public bool IsSurveyNotificationSent { get; set; }
        public bool HideDate { get; set; }
        public bool HideTime { get; set; }
        public bool HideLocation { get; set; }
        public bool HideMap { get; set; }
        public bool HideDescription { get; set; }
        public bool HidePhoto { get; set; }
        public bool HideAddNotes { get; set; }
        public bool HideAttendees { get; set; }
        public bool HideDocuments { get; set; }
        public bool HideSpeaker { get; set; }
        public bool HideSurvey { get; set; }
        public string WaiverFormTemplatePath { get; set; }
    
        public virtual ActivityTPYE ActivityTPYE { get; set; }
        public virtual Event Event { get; set; }
        public virtual FloorMapLocation FloorMapLocation { get; set; }
        public virtual FloorMapLocation FloorMapLocation1 { get; set; }
        public virtual FloorMapLocation FloorMapLocation2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookMark> BookMarks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Notes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QRCode> QRCodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QRCode> QRCodes1 { get; set; }
        public virtual FloorRegionMapping FloorRegionMapping { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Document> Documents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Survey> Surveys { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveySubmittedFor> SurveySubmittedFors { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExhibitorActivity> ExhibitorActivities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SponsorActivity> SponsorActivities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorActivity> VendorActivities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WaiverFormTemplateMapping> WaiverFormTemplateMappings { get; set; }
    }
}
