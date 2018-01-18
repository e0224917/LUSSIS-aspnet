using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class ManageCollectionDTO
    {
        public Disbursement Disbursement { get; set; }

        public List<CollectionPoint> CollectionPoint { get; set; }

        public IEnumerable<CollectionPoint> GetAll { get; set; }
    }
}