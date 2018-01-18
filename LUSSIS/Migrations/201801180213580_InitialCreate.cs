namespace LUSSIS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdjVoucher",
                c => new
                    {
                        AdjVoucherId = c.Int(nullable: false),
                        ItemNum = c.String(maxLength: 20),
                        ApprovalEmpNum = c.Int(),
                        Quantity = c.Int(),
                        Reason = c.String(maxLength: 50),
                        CreateDate = c.DateTime(storeType: "date"),
                        ApprovalDate = c.DateTime(storeType: "date"),
                        RequestEmpNum = c.Int(),
                        Status = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.AdjVoucherId)
                .ForeignKey("dbo.Employee", t => t.ApprovalEmpNum)
                .ForeignKey("dbo.Employee", t => t.RequestEmpNum)
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .Index(t => t.ItemNum)
                .Index(t => t.ApprovalEmpNum)
                .Index(t => t.RequestEmpNum);
            
            CreateTable(
                "dbo.Employee",
                c => new
                    {
                        EmpNum = c.Int(nullable: false),
                        Title = c.String(maxLength: 20),
                        FirstName = c.String(maxLength: 30),
                        LastName = c.String(maxLength: 30),
                        EmailAddress = c.String(maxLength: 50),
                        JobTitle = c.String(maxLength: 20),
                        DeptCode = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.EmpNum)
                .ForeignKey("dbo.Department", t => t.DeptCode)
                .Index(t => t.DeptCode);
            
            CreateTable(
                "dbo.CollectionPoint",
                c => new
                    {
                        CollectionPointId = c.Int(nullable: false),
                        CollectionName = c.String(maxLength: 30),
                        Time = c.String(maxLength: 20),
                        InChargeEmpNum = c.Int(),
                    })
                .PrimaryKey(t => t.CollectionPointId)
                .ForeignKey("dbo.Employee", t => t.InChargeEmpNum)
                .Index(t => t.InChargeEmpNum);
            
            CreateTable(
                "dbo.Department",
                c => new
                    {
                        DeptCode = c.String(nullable: false, maxLength: 20),
                        DeptName = c.String(maxLength: 50),
                        DeptHeadNum = c.Int(),
                        ContactName = c.String(maxLength: 50),
                        FaxNum = c.String(maxLength: 30),
                        TelephoneNum = c.String(maxLength: 30),
                        RepEmpNum = c.Int(),
                        CollectionPointId = c.Int(),
                    })
                .PrimaryKey(t => t.DeptCode)
                .ForeignKey("dbo.CollectionPoint", t => t.CollectionPointId)
                .ForeignKey("dbo.Employee", t => t.RepEmpNum)
                .Index(t => t.RepEmpNum)
                .Index(t => t.CollectionPointId);
            
            CreateTable(
                "dbo.Disbursement",
                c => new
                    {
                        DisbursementId = c.Int(nullable: false, identity: true),
                        CollectionDate = c.DateTime(storeType: "date"),
                        CollectionPointId = c.Int(),
                        DeptCode = c.String(maxLength: 20),
                        AcknowledgeEmpNum = c.Int(),
                        Status = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.DisbursementId)
                .ForeignKey("dbo.CollectionPoint", t => t.CollectionPointId)
                .ForeignKey("dbo.Department", t => t.DeptCode)
                .ForeignKey("dbo.Employee", t => t.AcknowledgeEmpNum)
                .Index(t => t.CollectionPointId)
                .Index(t => t.DeptCode)
                .Index(t => t.AcknowledgeEmpNum);
            
            CreateTable(
                "dbo.DisbursementDetail",
                c => new
                    {
                        DisbursementId = c.Int(nullable: false),
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        UnitPrice = c.Double(),
                        RequestedQty = c.Int(),
                        ActualQty = c.Int(),
                    })
                .PrimaryKey(t => new { t.DisbursementId, t.ItemNum })
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .ForeignKey("dbo.Disbursement", t => t.DisbursementId)
                .Index(t => t.DisbursementId)
                .Index(t => t.ItemNum);
            
            CreateTable(
                "dbo.Stationery",
                c => new
                    {
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        CategoryId = c.Int(),
                        Description = c.String(),
                        ReorderLevel = c.Int(),
                        ReorderQty = c.Int(),
                        AverageCost = c.Double(),
                        UnitOfMeasure = c.String(maxLength: 10),
                        CurrentQty = c.Int(),
                        BinNum = c.String(maxLength: 10),
                        AvailableQty = c.Int(),
                    })
                .PrimaryKey(t => t.ItemNum)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.PurchaseOrderDetail",
                c => new
                    {
                        PoNum = c.Int(nullable: false),
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        OrderQty = c.Int(),
                        UnitPrice = c.Double(),
                        ReceiveQty = c.Int(),
                    })
                .PrimaryKey(t => new { t.PoNum, t.ItemNum })
                .ForeignKey("dbo.PurchaseOrder", t => t.PoNum)
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .Index(t => t.PoNum)
                .Index(t => t.ItemNum);
            
            CreateTable(
                "dbo.PurchaseOrder",
                c => new
                    {
                        PoNum = c.Int(nullable: false, identity: true),
                        SupplierId = c.Int(),
                        CreateDate = c.DateTime(storeType: "date"),
                        OrderDate = c.DateTime(storeType: "date"),
                        Status = c.String(maxLength: 20),
                        ApprovalDate = c.DateTime(storeType: "date"),
                        OrderEmpNum = c.Int(),
                        ApprovalEmpNum = c.Int(),
                    })
                .PrimaryKey(t => t.PoNum)
                .ForeignKey("dbo.Supplier", t => t.SupplierId)
                .ForeignKey("dbo.Employee", t => t.OrderEmpNum)
                .ForeignKey("dbo.Employee", t => t.ApprovalEmpNum)
                .Index(t => t.SupplierId)
                .Index(t => t.OrderEmpNum)
                .Index(t => t.ApprovalEmpNum);
            
            CreateTable(
                "dbo.ReceiveTrans",
                c => new
                    {
                        ReceiveId = c.Int(nullable: false, identity: true),
                        PoNum = c.Int(),
                        ReceiveDate = c.DateTime(storeType: "date"),
                        InvoiceNum = c.String(maxLength: 30),
                        DeliveryOrderNum = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.ReceiveId)
                .ForeignKey("dbo.PurchaseOrder", t => t.PoNum)
                .Index(t => t.PoNum);
            
            CreateTable(
                "dbo.ReceiveTransDetail",
                c => new
                    {
                        ReceiveId = c.Int(nullable: false),
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        Quantity = c.Int(),
                    })
                .PrimaryKey(t => new { t.ReceiveId, t.ItemNum })
                .ForeignKey("dbo.ReceiveTrans", t => t.ReceiveId)
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .Index(t => t.ReceiveId)
                .Index(t => t.ItemNum);
            
            CreateTable(
                "dbo.Supplier",
                c => new
                    {
                        SupplierId = c.Int(nullable: false),
                        SupplierName = c.String(maxLength: 50),
                        ContactName = c.String(maxLength: 20),
                        TelephoneNum = c.String(maxLength: 20),
                        FaxNum = c.String(maxLength: 30),
                        Address = c.String(),
                        GstRegistration = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.SupplierId);
            
            CreateTable(
                "dbo.StationerySupplier",
                c => new
                    {
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        SupplierId = c.Int(nullable: false),
                        Price = c.Double(),
                        Rank = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItemNum, t.SupplierId })
                .ForeignKey("dbo.Supplier", t => t.SupplierId)
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .Index(t => t.ItemNum)
                .Index(t => t.SupplierId);
            
            CreateTable(
                "dbo.RequisitionDetail",
                c => new
                    {
                        RequisitionId = c.Int(nullable: false),
                        ItemNum = c.String(nullable: false, maxLength: 20),
                        Quantity = c.Int(),
                    })
                .PrimaryKey(t => new { t.RequisitionId, t.ItemNum })
                .ForeignKey("dbo.Requisition", t => t.RequisitionId)
                .ForeignKey("dbo.Stationery", t => t.ItemNum)
                .Index(t => t.RequisitionId)
                .Index(t => t.ItemNum);
            
            CreateTable(
                "dbo.Requisition",
                c => new
                    {
                        RequisitionId = c.Int(nullable: false, identity: true),
                        RequisitionEmpNum = c.Int(),
                        RequisitionDate = c.DateTime(storeType: "date"),
                        ApprovalEmpNum = c.Int(),
                        ApprovalRemarks = c.String(),
                        RequestRemarks = c.String(),
                        ApprovalDate = c.DateTime(storeType: "date"),
                        Status = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.RequisitionId)
                .ForeignKey("dbo.Employee", t => t.ApprovalEmpNum)
                .ForeignKey("dbo.Employee", t => t.RequisitionEmpNum)
                .Index(t => t.RequisitionEmpNum)
                .Index(t => t.ApprovalEmpNum);
            
            CreateTable(
                "dbo.Delegate",
                c => new
                    {
                        DelegateId = c.Int(nullable: false, identity: true),
                        EmpNum = c.Int(),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                    })
                .PrimaryKey(t => t.DelegateId)
                .ForeignKey("dbo.Employee", t => t.EmpNum)
                .Index(t => t.EmpNum);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requisition", "RequisitionEmpNum", "dbo.Employee");
            DropForeignKey("dbo.Requisition", "ApprovalEmpNum", "dbo.Employee");
            DropForeignKey("dbo.PurchaseOrder", "ApprovalEmpNum", "dbo.Employee");
            DropForeignKey("dbo.PurchaseOrder", "OrderEmpNum", "dbo.Employee");
            DropForeignKey("dbo.Disbursement", "AcknowledgeEmpNum", "dbo.Employee");
            DropForeignKey("dbo.Department", "RepEmpNum", "dbo.Employee");
            DropForeignKey("dbo.Delegate", "EmpNum", "dbo.Employee");
            DropForeignKey("dbo.CollectionPoint", "InChargeEmpNum", "dbo.Employee");
            DropForeignKey("dbo.Employee", "DeptCode", "dbo.Department");
            DropForeignKey("dbo.DisbursementDetail", "DisbursementId", "dbo.Disbursement");
            DropForeignKey("dbo.StationerySupplier", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.RequisitionDetail", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.RequisitionDetail", "RequisitionId", "dbo.Requisition");
            DropForeignKey("dbo.ReceiveTransDetail", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.PurchaseOrderDetail", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.StationerySupplier", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.PurchaseOrder", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.ReceiveTransDetail", "ReceiveId", "dbo.ReceiveTrans");
            DropForeignKey("dbo.ReceiveTrans", "PoNum", "dbo.PurchaseOrder");
            DropForeignKey("dbo.PurchaseOrderDetail", "PoNum", "dbo.PurchaseOrder");
            DropForeignKey("dbo.DisbursementDetail", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.Stationery", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.AdjVoucher", "ItemNum", "dbo.Stationery");
            DropForeignKey("dbo.Disbursement", "DeptCode", "dbo.Department");
            DropForeignKey("dbo.Disbursement", "CollectionPointId", "dbo.CollectionPoint");
            DropForeignKey("dbo.Department", "CollectionPointId", "dbo.CollectionPoint");
            DropForeignKey("dbo.AdjVoucher", "RequestEmpNum", "dbo.Employee");
            DropForeignKey("dbo.AdjVoucher", "ApprovalEmpNum", "dbo.Employee");
            DropIndex("dbo.Delegate", new[] { "EmpNum" });
            DropIndex("dbo.Requisition", new[] { "ApprovalEmpNum" });
            DropIndex("dbo.Requisition", new[] { "RequisitionEmpNum" });
            DropIndex("dbo.RequisitionDetail", new[] { "ItemNum" });
            DropIndex("dbo.RequisitionDetail", new[] { "RequisitionId" });
            DropIndex("dbo.StationerySupplier", new[] { "SupplierId" });
            DropIndex("dbo.StationerySupplier", new[] { "ItemNum" });
            DropIndex("dbo.ReceiveTransDetail", new[] { "ItemNum" });
            DropIndex("dbo.ReceiveTransDetail", new[] { "ReceiveId" });
            DropIndex("dbo.ReceiveTrans", new[] { "PoNum" });
            DropIndex("dbo.PurchaseOrder", new[] { "ApprovalEmpNum" });
            DropIndex("dbo.PurchaseOrder", new[] { "OrderEmpNum" });
            DropIndex("dbo.PurchaseOrder", new[] { "SupplierId" });
            DropIndex("dbo.PurchaseOrderDetail", new[] { "ItemNum" });
            DropIndex("dbo.PurchaseOrderDetail", new[] { "PoNum" });
            DropIndex("dbo.Stationery", new[] { "CategoryId" });
            DropIndex("dbo.DisbursementDetail", new[] { "ItemNum" });
            DropIndex("dbo.DisbursementDetail", new[] { "DisbursementId" });
            DropIndex("dbo.Disbursement", new[] { "AcknowledgeEmpNum" });
            DropIndex("dbo.Disbursement", new[] { "DeptCode" });
            DropIndex("dbo.Disbursement", new[] { "CollectionPointId" });
            DropIndex("dbo.Department", new[] { "CollectionPointId" });
            DropIndex("dbo.Department", new[] { "RepEmpNum" });
            DropIndex("dbo.CollectionPoint", new[] { "InChargeEmpNum" });
            DropIndex("dbo.Employee", new[] { "DeptCode" });
            DropIndex("dbo.AdjVoucher", new[] { "RequestEmpNum" });
            DropIndex("dbo.AdjVoucher", new[] { "ApprovalEmpNum" });
            DropIndex("dbo.AdjVoucher", new[] { "ItemNum" });
            DropTable("dbo.Delegate");
            DropTable("dbo.Requisition");
            DropTable("dbo.RequisitionDetail");
            DropTable("dbo.StationerySupplier");
            DropTable("dbo.Supplier");
            DropTable("dbo.ReceiveTransDetail");
            DropTable("dbo.ReceiveTrans");
            DropTable("dbo.PurchaseOrder");
            DropTable("dbo.PurchaseOrderDetail");
            DropTable("dbo.Category");
            DropTable("dbo.Stationery");
            DropTable("dbo.DisbursementDetail");
            DropTable("dbo.Disbursement");
            DropTable("dbo.Department");
            DropTable("dbo.CollectionPoint");
            DropTable("dbo.Employee");
            DropTable("dbo.AdjVoucher");
        }
    }
}
