namespace LUSSIS.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class LUSSISContext : DbContext
    {
        public LUSSISContext()
            : base("name=LUSSISContext")
        {
        }

        public virtual DbSet<AdjVoucher> AdjVouchers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CollectionPoint> CollectionPoints { get; set; }
        public virtual DbSet<Delegate> Delegates { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Disbursement> Disbursements { get; set; }
        public virtual DbSet<DisbursementDetail> DisbursementDetails { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public virtual DbSet<ReceiveTran> ReceiveTrans { get; set; }
        public virtual DbSet<ReceiveTransDetail> ReceiveTransDetails { get; set; }
        public virtual DbSet<Requisition> Requisitions { get; set; }
        public virtual DbSet<RequisitionDetail> RequisitionDetails { get; set; }
        public virtual DbSet<Stationery> Stationeries { get; set; }
        public virtual DbSet<StationerySupplier> StationerySuppliers { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>()
                .HasMany(e => e.Employees)
                .WithOptional(e => e.Department)
                .HasForeignKey(e => e.DeptCode);

            modelBuilder.Entity<Disbursement>()
                .HasMany(e => e.DisbursementDetails)
                .WithRequired(e => e.Disbursement)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.AdjVouchers)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ApprovalEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.AdjVouchers1)
                .WithOptional(e => e.Employee1)
                .HasForeignKey(e => e.RequestEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.CollectionPoints)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.InChargeEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Departments)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.RepEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Disbursements)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.AcknowledgeEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.PurchaseOrders)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.OrderEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.PurchaseOrders1)
                .WithOptional(e => e.Employee1)
                .HasForeignKey(e => e.ApprovalEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Requisitions)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.ApprovalEmpNum);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Requisitions1)
                .WithOptional(e => e.Employee1)
                .HasForeignKey(e => e.RequisitionEmpNum);

            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(e => e.PurchaseOrderDetails)
                .WithRequired(e => e.PurchaseOrder)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ReceiveTran>()
                .HasMany(e => e.ReceiveTransDetails)
                .WithRequired(e => e.ReceiveTran)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Requisition>()
                .HasMany(e => e.RequisitionDetails)
                .WithRequired(e => e.Requisition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stationery>()
                .HasMany(e => e.DisbursementDetails)
                .WithRequired(e => e.Stationery)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stationery>()
                .HasMany(e => e.PurchaseOrderDetails)
                .WithRequired(e => e.Stationery)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stationery>()
                .HasMany(e => e.ReceiveTransDetails)
                .WithRequired(e => e.Stationery)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stationery>()
                .HasMany(e => e.RequisitionDetails)
                .WithRequired(e => e.Stationery)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stationery>()
                .HasMany(e => e.StationerySuppliers)
                .WithRequired(e => e.Stationery)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Supplier>()
                .HasMany(e => e.StationerySuppliers)
                .WithRequired(e => e.Supplier)
                .WillCascadeOnDelete(false);
        }
    }
}
