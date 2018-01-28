using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class CategoryRepository : Repository<Category, int>
    {

        public IEnumerable<SelectListItem> GetCategories()
        {
            return LUSSISContext.Categories.ToList().Select(x => new SelectListItem
            {
                Text = x.CategoryName,
                Value = x.CategoryId.ToString()
            });
        }

        public IEnumerable<String> GetAllCategoryName()
        {
            return LUSSISContext.Categories.Select(x => x.CategoryName);
        }

        public List<Category> GetCategoryBySupplier(String supplier)
        {
            List<Category> cat = new List<Category>();
            int id = Convert.ToInt32(supplier);
            var q = (from t1 in LUSSISContext.Stationeries
                     join t2 in LUSSISContext.StationerySuppliers
                     on t1.ItemNum equals t2.ItemNum
                     where t2.Supplier.SupplierId == id
                     select new { categoryId = t1.CategoryId }).Distinct();

            foreach (var a in q)
            {

                int catid = (int)a.categoryId;
                cat.Add(LUSSISContext.Categories.Where(x => x.CategoryId == catid).FirstOrDefault());
            }
            return cat;
        }

        public List<int> GetAllCategoryIds()
        {
            return LUSSISContext.Categories.Select(x => x.CategoryId).ToList();
        }


        public List<String> GetCategoryNameById(List<String> ids)
        {
            List<String> list = new List<String>();
            foreach (String id in ids)
            {
                int idCat = Convert.ToInt32(id);
                Category c = (LUSSISContext.Categories.Where(x => x.CategoryId == idCat).FirstOrDefault());
                list.Add(c.CategoryName);
            }
            return list;
        }
    }
}