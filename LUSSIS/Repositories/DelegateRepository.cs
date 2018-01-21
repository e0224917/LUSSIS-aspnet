using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Repositories
{
    public class DelegateRepository : Repository<Models.Delegate, int>
    {
        public void DeleteDelegate(Models.Delegate del)
        {
            LUSSISContext.Delegates.Remove(del);
            LUSSISContext.SaveChanges();
        }
    }
}