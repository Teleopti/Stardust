using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    public class QueryFilterTest : DatabaseTest
    {
        //[Test]
        //public void VerifyBusinessUnit()
        //{
        //    Assert.AreEqual("businessUnitFilter", QueryFilter.BusinessUnit.Name);
        //    Assert.IsNotNull(Session.SessionFactory.GetFilterDefinition(QueryFilter.BusinessUnit.Name));
        //}

        [Test]
        public void VerifyFilterNamesExistsInMapping()
        {
            Assert.AreEqual("deletedFlagFilter", QueryFilter.Deleted.Name);
            Assert.IsNotNull(Session.SessionFactory.GetFilterDefinition(QueryFilter.Deleted.Name));
        }

        protected override void SetupForRepositoryTest()
        {
            //nothing
        }
    }
}
