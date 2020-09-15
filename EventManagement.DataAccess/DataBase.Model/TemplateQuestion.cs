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
    
    public partial class TemplateQuestion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TemplateQuestion()
        {
            this.TemplateQuestionOptions = new HashSet<TemplateQuestionOption>();
        }
    
        public int TQuestionID { get; set; }
        public string TQuestionText { get; set; }
        public int TQuestionResponseType { get; set; }
        public Nullable<int> TQSurveyID { get; set; }
    
        public virtual ResponseType ResponseType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TemplateQuestionOption> TemplateQuestionOptions { get; set; }
        public virtual TemplateSurvey TemplateSurvey { get; set; }
    }
}
