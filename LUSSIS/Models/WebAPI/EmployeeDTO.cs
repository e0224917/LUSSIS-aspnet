using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class EmployeeDTO
    {
        public int EmpNum { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string JobTitle { get; set; }

        public string DeptCode { get; set; }

        public string DeptName { get; set; }

        public bool IsDelegated { get; set; }

    }
}