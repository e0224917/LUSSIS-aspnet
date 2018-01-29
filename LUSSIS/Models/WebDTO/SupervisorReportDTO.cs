using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Models.WebDTO
{
    //Authors: May Zin Ko
    public class SupervisorReportDTO
    {
        public List<Supplier> Suppliers { get; set; }
        public List<Category> Categories { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsChart { get; set; }

        public List<Department> Department { get; set; }
        
    }
}