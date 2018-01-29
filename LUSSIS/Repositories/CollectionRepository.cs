using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    //Authors: Ong Xin Ying
    public class CollectionRepository : Repository<CollectionPoint, int>
    {
        public CollectionPoint GetCollectionPointByDeptCode(string deptCode)
        {
            Department d = LUSSISContext.Departments.First(z => z.DeptCode == deptCode);
            return LUSSISContext.CollectionPoints.First(x => x.CollectionPointId == d.CollectionPointId);
        }
    }
}