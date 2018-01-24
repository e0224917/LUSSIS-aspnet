using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class StationeryDTO
    {
    
        public virtual Stationery stationery { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }


        [Required(ErrorMessage = "Choose Different Supplier")]
        public string SupplierName1 { get; set; }

        [Required(ErrorMessage = "Choose Different Supplier")]
        public string SupplierName2 { get; set; }

        [Required(ErrorMessage = "Choose Different Supplier")]
        public string SupplierName3 { get; set; }

        [Required(ErrorMessage = "Choose Different Supplier")]
        public int SupplierId1 { get; set; }

        [Required(ErrorMessage = "Choose Different Supplier")]
        public int SupplierId2 { get; set; }

        [Required(ErrorMessage = "Choose Different Supplier")]
        public int SupplierId3 { get; set; }

        //public double? Price { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double? Price1 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double? Price2 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double? Price3 { get; set; }

        public int? Rank { get; set; }

        [Required(ErrorMessage = "This SupplierName is required")]
        public string SupplierName { get; set; }

        public DateTime? ReceiveDate { get; set; }

        [Range(1, 10000, ErrorMessage = "Please enter a valid quantity")]
        public int? Quantity { get; set; }

        public int? CurrentQty { get; set; }

        public string TransactioType { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public int? ActualQty { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public int? Quantity1 { get; set; }

    }
}