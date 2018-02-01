using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Koh Meng Guan
    [NotMapped]
    public class AdjustmentVoucherDTO : IValidatableObject
    {


        [ItemNumValidator]
        [Display(Name = "Item Code")]
        [Required(ErrorMessage = "This field is required")]
        [StringLength(20)]
        public string ItemNum { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [Range (1,10000, ErrorMessage="Please enter a valid  between 1 to 10000")]
        public int Quantity { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(50)]
        public string Reason { get; set; }



        public virtual Stationery Stationery { get; set; }

        [Display(Name="Adjustment Type")]
        [Required(ErrorMessage = "This field is required")]
        public bool? Sign { get; set; }

        public IList<AdjustmentVoucherDTO> MyList { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            StationeryRepository sr = new StationeryRepository();
            var ItemCurrentQuantity = sr.GetById(ItemNum).CurrentQty;
            var isPositive = true ;
            if (Sign != null) {isPositive= Convert.ToBoolean(Sign); }
            if ( !isPositive && Quantity > ItemCurrentQuantity)
            {
                yield return new ValidationResult("Number exceed current quantity", new List<string> { "Quantity" });
            }
        }

    }

    public class ItemNumValidator : ValidationAttribute
    {
        public ItemNumValidator(): base("Invalid item code")
        {

        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
 
            if (value != null)
            {
                var valueAsString = value.ToString();
                StationeryRepository sr = new StationeryRepository();
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