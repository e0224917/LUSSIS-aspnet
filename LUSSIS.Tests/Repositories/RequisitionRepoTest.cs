using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LUSSIS;
using LUSSIS.Models;
using LUSSIS.Repositories;
using LUSSIS.Repositories.Interface;


namespace LUSSIS.Tests.Repositories
{
    [TestClass]
    public class RequisitionRepoTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            RequisitionRepository reqrepo = new RequisitionRepository();
        }
    }
}
