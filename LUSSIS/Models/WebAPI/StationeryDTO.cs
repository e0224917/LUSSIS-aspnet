using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class StationeryDTO
    {
        public string ItemNum { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public int? ReorderLevel { get; set; }

        public int? ReorderQty { get; set; }

        public int? AvailableQty { get; set; }

        public string UnitOfMeasure { get; set; }

        public string BinNum { get; set; }

    }
}