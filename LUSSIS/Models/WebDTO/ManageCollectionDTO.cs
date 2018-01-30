using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Ong Xin Ying
    public class ManageCollectionDTO
    {
        public CollectionPoint CollectionPoint { get; set; }

        public int DeptCollectionPointId { get; set; }

        public IEnumerable<CollectionPoint> CollectionPoints { get; set; }
    }
}