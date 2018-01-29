using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: May Zin Ko
    public class ReportTransferDTO
    {
        public String title { get; set; }
       public String timeValue { get; set; }

       public List<double> xvalue { get; set; }
    }
}