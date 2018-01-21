using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class RequisitionDetailDTO
    {
        public string Description { get; set; }

        public string UnitOfMeasure { get; set; }

        public int Quantity { get; set; }

    }
}