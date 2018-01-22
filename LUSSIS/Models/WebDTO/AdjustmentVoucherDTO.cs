using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    [NotMapped]
    public class AdjustmentVoucherDTO
    {
        [ItemNumValidator]
        [Required(ErrorMessage = "This field is required")]
        [StringLength(20)]
        public string ItemNum { get; set; }

        [Range (1,10000, ErrorMessage="Please enter a valid quantity")]
        public int Quantity { get; set; }


        [StringLength(50)]
        public string Reason { get; set; }



        public virtual Stationery Stationery { get; set; }

        [Display(Name="Adjustment Type")]
        [Required(ErrorMessage = "This field is required")]
        public bool? Sign { get; set; }

        public IList<AdjustmentVoucherDTO> MyList { get; set; }

    }

    public class ItemNumValidator : ValidationAttribute
    {
        public ItemNumValidator(): base("Invalid item code")
        {

        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            StationeryRepository sr = new StationeryRepository();
            if (value != null)
            {
                var valueAsString = value.ToString();

                if (!sr.GetAllItemNum().ToList().Contains(valueAsString))
                {
                    var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                    return new ValidationResult(errorMessage);
                }
                
            }
            return ValidationResult.Success;
        }
    }
}