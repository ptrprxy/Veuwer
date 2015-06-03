namespace Veuwer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBlob : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ImageLinks", "VoatLink");
            DropColumn("dbo.Images", "ImgBlob");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Images", "ImgBlob", c => c.Binary());
            AddColumn("dbo.ImageLinks", "VoatLink", c => c.String());
        }
    }
}
