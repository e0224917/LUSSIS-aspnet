using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Models.WebDTO
{
    public class StationerynDTO
    {
        public string ItemNum { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [Required]
        public int ReorderLevel { get; set; }

        [Required]
        public int ReorderQty { get; set; }

        [Required]
        public int BinNum { get; set; }

        [Required]
        [StringLength(10)]
        public string UnitOfMeasure { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        public string SupplierName1 { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        public string SupplierName2 { get; set; }

        [Required(ErrorMessage = "Please choose a supplier")]
        public string SupplierName3 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double Price1 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double Price2 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double Price3 { get; set; }
    }
}