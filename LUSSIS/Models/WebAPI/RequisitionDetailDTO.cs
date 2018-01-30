using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class RequisitionDetailDTO
    {
        public string ItemNum { get; set; }

        public string Description { get; set; }

        public string UnitOfMeasure { get; set; }

        public int Quantity { get; set; }

    }
}