using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LUSSIS.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CollectionDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime collectionDate = Convert.ToDateTime(value);
                if (collectionDate < DateTime.Now)
                {
                    return new ValidationResult("Collection date must be greater than today.");
                }
            }
            return ValidationResult.Success;
        }

    }
}