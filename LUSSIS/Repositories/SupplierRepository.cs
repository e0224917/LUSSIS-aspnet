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

        public IEnumerable<SelectListItem> GetSupplierList()
        {
            return LUSSISContext.Suppliers.ToList().Select(x => new SelectListItem
            {
                Text = x.SupplierName,
                Value = x.SupplierId.ToString()
            });
        }


        public List<Supplier> GetSupplierByCategory(String category)
        {
            List<Supplier> supList = new List<Supplier>();
            int id = Convert.ToInt32(category);
            var q = (from t1 in LUSSISContext.Stationeries
                     join t2 in LUSSISContext.StationerySuppliers
                     on t1.ItemNum equals t2.ItemNum
                     where t1.CategoryId == id
                     select new { supplierId = t2.SupplierId }).Distinct();

            foreach (var a in q)
            {
                int supId = (int)a.supplierId;
                supList.Add(LUSSISContext.Suppliers.Where(x => x.SupplierId == supId).FirstOrDefault());
            }
            return supList;
        }
    }
}