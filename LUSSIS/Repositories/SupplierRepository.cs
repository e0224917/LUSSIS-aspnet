using LUSSIS.Models;
using LUSSIS.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LUSSIS.Repositories
{
    //Authors: Ton That Minh Nhat
    public class SupplierRepository : Repository<Supplier, int>, ISupplierRepository
    {
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