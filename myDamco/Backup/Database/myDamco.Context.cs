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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class myDamcoEntities : DbContext
    {
        public myDamcoEntities()
            : base("name=myDamcoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Downtime> Downtime { get; set; }
        public DbSet<ELMAH_Error> ELMAH_Error { get; set; }
        public DbSet<Navigation> Navigation { get; set; }
        public DbSet<NewsCategory> NewsCategory { get; set; }
        public DbSet<NewsItem> NewsItem { get; set; }
        public DbSet<Page> Page { get; set; }
        public DbSet<Widget> Widget { get; set; }
        public DbSet<WidgetInstance> WidgetInstance { get; set; }
        public DbSet<WidgetInstanceHistory> WidgetInstanceHistory { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<DashboardTemplate> DashboardTemplate { get; set; }
    }
}
