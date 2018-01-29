namespace LUSSIS.Constants
{
    //Authors: Ton That Minh Nhat
    public class RequisitionStatus
    {
        public const string InProgress = "inprogress";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
    }

    public class DisbursementStatus
    {
        public const string Fulfilled = "fulfilled";
        public const string Unfulfilled = "unfulfilled";

    }

    public class AdjustmentVoucherStatus
    {
        public const string Pending = "pending";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
    }

    public class POStatus
    {
        public const string Pending = "pending";
        public const string Ordered = "ordered";
        public const string Approved = "approved";
    }
}