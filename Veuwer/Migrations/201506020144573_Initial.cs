namespace Veuwer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImageLinks",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        VoatLink = c.String(),
                        Image_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Images", t => t.Image_Id)
                .Index(t => t.Image_Id);

            CreateTable(
                "dbo.Images",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ImgBlob = c.Binary(),
                        Hash = c.String(),
                        MimeType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImageLinks", "Image_Id", "dbo.Images");
            DropIndex("dbo.ImageLinks", new[] { "Image_Id" });
            DropTable("dbo.Images");
            DropTable("dbo.ImageLinks");
        }
    }
}
