namespace HospitalManagementSystemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HospitalModelChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Hospitals", "Address", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Hospitals", "Address", c => c.String(nullable: false));
        }
    }
}
