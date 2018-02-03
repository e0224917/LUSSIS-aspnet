using LUSSIS.Models;
using LUSSIS.Models.Account;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LUSSIS.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("name=LUSSISContext")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}