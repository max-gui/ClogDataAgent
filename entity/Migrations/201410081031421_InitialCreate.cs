namespace entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogInfoes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        AppId = c.String(),
                        Level = c.Int(nullable: false),
                        HostIp = c.Int(nullable: false),
                        message = c.String(),
                        Uid = c.String(),
                        Platform = c.String(),
                        Serviceversion = c.String(),
                        Servicecode = c.String(),
                        Servicetype = c.String(),
                        Guid = c.String(),
                        Logtype = c.String(),
                        Bustype = c.String(),
                        Orderid = c.String(),
                        Title = c.String(),
                        head = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.MoreInfoes",
                c => new
                    {
                        url = c.String(nullable: false, maxLength: 128),
                        head = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.url);
            
            CreateTable(
                "dbo.RetryInfoes",
                c => new
                    {
                        url = c.String(nullable: false, maxLength: 128),
                        head = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.url);
            
            CreateTable(
                "dbo.TimeTables",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        start = c.DateTime(nullable: false),
                        end = c.DateTime(nullable: false),
                        head = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TimeTables");
            DropTable("dbo.RetryInfoes");
            DropTable("dbo.MoreInfoes");
            DropTable("dbo.LogInfoes");
        }
    }
}
