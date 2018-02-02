using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Koh Meng Guan
    public class StationeryDTO
    {
        public string ItemNum { get; set; }

        [Required(ErrorMessage ="Please choose a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [Required(ErrorMessage ="Reorder Level is required")]
        [Display(Name = "Reorder Level")]
        [Range(1,Int32.MaxValue, ErrorMessage = ("Reorder Level cannot be less than 0"))]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage ="Reorder Quantity is required")]
        [Display(Name = "Reorder Quantity")]
        [Range(1, Int32.MaxValue,ErrorMessage =("Reorder Quantity cannot be less than 0"))]
        public int ReorderQty { get; set; }

        [Required(ErrorMessage ="Bin Number is required")]
        [Display(Name = "Bin Number")]
        [RegularExpression(@"^[a-zA-Z][1-9]$", ErrorMessage ="Bin number must be in the format of an alphabet followed by a number e.g.(B9)")]
        public string BinNum { get; set; }

        [Required(ErrorMessage ="Unit of Measure is required")]
        [Display(Name = "Unit of Measure")]
        [StringLength(10, ErrorMessage ="Max Length of 10 characters allowed")]
        public string UnitOfMeasure { get; set; }

        public IEnumerable<SelectListItem> UomList
        {
            get
            {
                var list = new List<SelectListItem>
                {
                    new SelectListItem()
                    {
                       Text = "Box", Value = "Box"
                    },
                    new SelectListItem()
                    {
                       Text = "Dozen", Value = "Dozen"
                    },
                    new SelectListItem()
                    {
                       Text = "Each", Value = "Each"
                    },
                    new SelectListItem()
                    {
                       Text = "Packet", Value = "Packet"
                    },
                    new SelectListItem()
                    {
                       Text = "Set", Value = "Set"
                    },
                };
                return list;
            }
        }

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
        [Range(0, Int32.MaxValue, ErrorMessage = ("Price cannot be less than 0"))]
        public double Price1 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Rank 2 Supplier Price")]
        [Range(0, Int32.MaxValue, ErrorMessage = ("Price  cannot be less than 0"))]
        public double Price2 { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Rank 3 Supplier Price")]
        [Range(0, Int32.MaxValue, ErrorMessage = ("Price cannot less than 0"))]
        public double Price3 { get; set; }
    }
}