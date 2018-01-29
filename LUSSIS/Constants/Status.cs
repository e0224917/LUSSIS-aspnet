namespace LUSSIS.Constants
{
    public class RequisitionStatus
    {
        public const string InProcess = "inprocess";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
        public const string Processed = "processed";
    }

    public class DisbursementStatus
    {
        public const string InProcess = "inprocess";
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