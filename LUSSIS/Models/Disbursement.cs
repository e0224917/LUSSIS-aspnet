using System.ComponentModel;
using System.Linq;
using LUSSIS.Models.WebAPI;
using LUSSIS.Validations;

namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Disbursement")]
    public partial class Disbursement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Disbursement()
        {
            DisbursementDetails = new HashSet<DisbursementDetail>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DisbursementId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [Display(Name = "Collection Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        //[CollectionDate] //validation
        public DateTime CollectionDate { get; set; }

        //[Required]
        [Display(Name = "Collection Point Id")]
        public int? CollectionPointId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Department Code")]
        public string DeptCode { get; set; }

        public int? AcknowledgeEmpNum { get; set; }

        [Required] [StringLength(20)] public string Status { get; set; }

        [Display(Name = "Collection Point")] public virtual CollectionPoint CollectionPoint { get; set; }

        public virtual Department Department { get; set; }

        public virtual Employee AcknowledgeEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DisbursementDetail> DisbursementDetails { get; set; }

        public Disbursement(List<RequisitionDetail> requisitionDetails, DateTime collectionDate)
        {
            var department = requisitionDetails.First().Requisition.RequisitionEmployee.Department;
            Status = "inprocess";
            CollectionDate = collectionDate;
            DeptCode = department.DeptCode;
            CollectionPointId = department.CollectionPointId;

            DisbursementDetails = new HashSet<DisbursementDetail>();
            foreach (var requisitionDetail in requisitionDetails)
            {
                //Convert from requisitionDetail to disbursementDetail 
                var disbursementDetail = new DisbursementDetail(requisitionDetail);
                Add(disbursementDetail);
            }

            Count = DisbursementDetails.Count;
        }

        [NotMapped]
        public int Count { get; set; }

        /// <summary>
        /// If disbursment detail already existed, increase the requested qty, 
        /// else add as a new disbursement detail
        /// </summary>
        /// <param name="item"></param>
        public void Add(DisbursementDetail item)
        {
            if (DisbursementDetails.Count > 0)
            {
                var isNew = true;
                for (int i = 0; i < Count; i++)
                {
                    if (item.ItemNum == DisbursementDetails.ElementAt(i).ItemNum)
                    {
                        DisbursementDetails.ElementAt(i).RequestedQty += item.RequestedQty;
                        //check if enough in stock again, if not set actual qty equal to available in stock
                        DisbursementDetails.ElementAt(i).ActualQty = item.Stationery.AvailableQty > DisbursementDetails.ElementAt(i).RequestedQty
                            ? DisbursementDetails.ElementAt(i).RequestedQty
                            : item.Stationery.AvailableQty;
                        isNew = false;
                        break;
                    }
                }
                if (isNew)
                {
                    DisbursementDetails.Add(item);
                    Count++;
                }
            }
            else
            {
                DisbursementDetails.Add(item);
                Count++;
            }
            
        }

        public Disbursement(Disbursement unfulfilledDisbursement, DateTime collectionDate)
        {
            DeptCode = unfulfilledDisbursement.DeptCode;
            Status = "inprocess";
            CollectionDate = collectionDate;
            CollectionPointId = unfulfilledDisbursement.Department.CollectionPointId;

            Count = 0;
        }
    }
}