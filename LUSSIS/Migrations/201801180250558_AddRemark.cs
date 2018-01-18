namespace LUSSIS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRemark : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdjVoucher", "Remark", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdjVoucher", "Remark");
        }
    }
}
