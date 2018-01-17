using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUSSIS.Repositories
{
    interface IStationeryRepository : IRepository<Stationery, string>
    {
        IEnumerable<Stationery> GetByCategory(string Categorry);
    }
}
