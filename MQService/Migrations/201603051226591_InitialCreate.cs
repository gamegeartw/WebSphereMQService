namespace MQService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MQDatas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MsgID = c.String(maxLength: 4000),
                        CorrelationId = c.String(maxLength: 4000),
                        GroupID = c.String(maxLength: 4000),
                        Msg = c.String(maxLength: 4000),
                        GetDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MQDatas");
        }
    }
}
