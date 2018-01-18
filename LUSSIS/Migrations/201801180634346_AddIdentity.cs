namespace LUSSIS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdentity : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Delegate", "DelegateId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Disbursement", "DisbursementId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Employee", "EmpNum", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.PurchaseOrder", "PoNum", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ReceiveTrans", "ReceiveId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Supplier", "SupplierId", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Delegate", "DelegateId", c => c.Int(nullable: false));
            AlterColumn("dbo.Disbursement", "DisbursementId", c => c.Int(nullable: false));
            AlterColumn("dbo.Employee", "EmpNum", c => c.Int(nullable: false));
            AlterColumn("dbo.PurchaseOrder", "PoNum", c => c.Int(nullable: false));
            AlterColumn("dbo.ReceiveTrans", "ReceiveId", c => c.Int(nullable: false));
            AlterColumn("dbo.Supplier", "SupplierId", c => c.Int(nullable: false));
        }
    }
}
