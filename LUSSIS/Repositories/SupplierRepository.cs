using LUSSIS.Models;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Repositories
{
    public class SupplierRepository : Repository<Supplier, int>, ISupplierRepository
    {
        public SupplierRepository()
        {
        }

        public IEnumerable<SelectListItem> GetSupplierList()
        {
            return LUSSISContext.Suppliers.ToList().Select(x => new SelectListItem
            {
                Text = x.SupplierName,
                Value = x.SupplierId.ToString()
            });
        }

    }
}