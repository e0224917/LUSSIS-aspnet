namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class AdjustmentDTO
    {
        public string ItemNum { get; set; }

        public int Quantity { get; set; }

        public string Reason { get; set; }

        public int RequestEmpNum { get; set; }

        public int ApprovalEmpNum { get; set; }

        public string Remark { get; set; }
    }
}