using System;

namespace LUSSIS.Models.WebAPI
{
    public class DelegateDTO
    {
        public int DelegateId { get; set; }

        public EmployeeDTO Employee { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}