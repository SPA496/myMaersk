//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myDamco.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class NewsItem
    {
        public NewsItem()
        {
            this.Downtime = new HashSet<Downtime>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public System.DateTime From { get; set; }
        public Nullable<System.DateTime> To { get; set; }
        public int NewsCategory_Id { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    
        public virtual ICollection<Downtime> Downtime { get; set; }
        public virtual NewsCategory NewsCategory { get; set; }
    }
}
