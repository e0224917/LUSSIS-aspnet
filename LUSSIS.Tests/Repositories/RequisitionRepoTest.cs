using System;
using System.Collections.Generic;
using System.Diagnostics;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LUSSIS.Tests.Repositories
{
    [TestClass]
    public class RequisitionRepoTest
    {
        [TestMethod]
        public void GetConsolidated()
        {
            // Arrange
            RequisitionRepository repo = new RequisitionRepository();

            // Act
            List<RetrievalItemDTO> dto = repo.GetConsolidatedRequisition();

            // Assert
            foreach (RetrievalItemDTO d in dto)
            {
                Debug.WriteLine(d.Description);
            }
            Assert.IsNotNull(dto);
        }

    }
}
