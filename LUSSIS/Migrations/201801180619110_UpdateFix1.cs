namespace LUSSIS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFix1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AdjVoucher", "AdjVoucherId", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AdjVoucher", "AdjVoucherId", c => c.Int(nullable: false));
        }
    }
}
