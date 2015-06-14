namespace Veuwer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PageViews : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PageViews",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        IP = c.String(),
                        Timestamp = c.DateTime(nullable: false),
                        Page = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PageViews");
        }
    }
}
