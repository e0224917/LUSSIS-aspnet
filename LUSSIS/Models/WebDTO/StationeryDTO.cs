using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Models.WebDTO
{
    public class StationeryDTO
    {
        public string ItemNum { get; set; }

        [Required(ErrorMessage ="Please choose a category")]
        [Display(Name = "Category")]
        public string CategoryId { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [Required(ErrorMessage ="Reorder Level is required")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage ="Reorder Quantity is required")]
        [Display(Name = "Reorder Quantity")]
        public int ReorderQty { get; set; }

        [Required(ErrorMessage ="Bin Number is required")]
        [Display(Name = "Bin Number")]
        [RegularExpression(@"^[a-zA-Z][1-9]$", ErrorMessage ="Bin number must be in the format of an alphabet followed by a number e.g.(B9)")]
        public string BinNum { get; set; }

        [Required(ErrorMessage ="Unit of Measure is required")]
        [Display(Name = "Unit of Measure")]
        [StringLength(10, ErrorMessage ="Max Length of 10 characters allowed")]
        public string UnitOfMeasure { get; set; }

        [Required(ErrorMessage ="Item Description is required")]
        [Display(Name = "Item Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        [Display(Name = "Rank 1 Supplier")]
        public string SupplierName1 { get; set; }

        public IEnumerable<SelectListItem> SupplierList { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        [Display(Name = "Rank 2 Supplier")]
        public string SupplierName2 { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        [Display(Name = "Rank 3 Supplier")]
        public string SupplierName3 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Rank 1 Supplier Price")]
        public double Price1 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Rank 2 Supplier Price")]
        public double Price2 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Rank 3 Supplier Price")]
        public double Price3 { get; set; }
    }
}