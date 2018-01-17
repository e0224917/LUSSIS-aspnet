using LUSSIS.Models;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LUSSIS.Repositories
{
    public class SupplierRepository : Repository<Supplier, int>, ISupplierRepository
    {
        public SupplierRepository(LUSSISContext context) : base(context)
        {
        }

        public LUSSISContext LUSSISContext
        {
            get { return Context as LUSSISContext; }
        }
    }
}