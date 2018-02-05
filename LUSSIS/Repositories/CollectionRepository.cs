using System.Linq;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    //Authors: Ong Xin Ying
    public class CollectionRepository : Repository<CollectionPoint, int>
    {
        public CollectionPoint GetCollectionPointByDeptCode(string deptCode)
        {
            var department = LUSSISContext.Departments.First(z => z.DeptCode == deptCode);
            return LUSSISContext.CollectionPoints.First(x => x.CollectionPointId == department.CollectionPointId);
        }
    }
}