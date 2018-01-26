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
        public SupplierRepository()
        {
        }
        public List<String>GetSupplierNamebyId(List<String>suppId)
        {
            List<String> list = new List<String>();
            foreach (String idSup in suppId)
            {
                int id= Convert.ToInt32(idSup);
                list.Add((LUSSISContext.Suppliers.Where(x => x.SupplierId==id).FirstOrDefault()).SupplierName);
            }
            return list;
        }

    }
}