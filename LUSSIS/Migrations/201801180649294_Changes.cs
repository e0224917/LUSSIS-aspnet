namespace LUSSIS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ReceiveTransDetail", "ReceiveId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ReceiveTransDetail", "ReceiveId", c => c.Int(nullable: false, identity: true));
        }
    }
}
