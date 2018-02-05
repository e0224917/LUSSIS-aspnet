using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    //Authors: Koh Meng Guan
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

        public IEnumerable<string> GetAllCategoryName()
        {
            return LUSSISContext.Categories.Select(x => x.CategoryName);
        }

        public List<Category> GetCategoryBySupplier(string supplier)
        {
            var categories = new List<Category>();
            var id = Convert.ToInt32(supplier);
            var query = (from t1 in LUSSISContext.Stationeries
                join t2 in LUSSISContext.StationerySuppliers
                    on t1.ItemNum equals t2.ItemNum
                where t2.Supplier.SupplierId == id
                select new {categoryId = t1.CategoryId}).Distinct();

            foreach (var category in query)
            {
                var categoryId = category.categoryId;
                categories.Add(LUSSISContext.Categories.FirstOrDefault(x => x.CategoryId == categoryId));
            }

            return categories;
        }

        public List<int> GetAllCategoryIds()
        {
            return LUSSISContext.Categories.Select(x => x.CategoryId).ToList();
        }

        public List<string> GetCategoryNameById(List<string> ids)
        {
            var list = new List<string>();
            foreach (var id in ids)
            {
                var idCat = Convert.ToInt32(id);
                var category = GetById(idCat);
                list.Add(category.CategoryName);
            }

            return list;
        }
    }
}