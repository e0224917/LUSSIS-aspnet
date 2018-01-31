using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class EmployeeDTO
    {
        public EmployeeDTO(Employee employee)
        {
            DeptCode = employee.DeptCode;
            DeptName = employee.Department.DeptName;
            EmailAddress = employee.EmailAddress;
            EmpNum = employee.EmpNum;
            FirstName = employee.FirstName;
            IsDelegated = false;
            JobTitle = employee.JobTitle;
            LastName = employee.LastName;
            Title = employee.Title;
        }

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