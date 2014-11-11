using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    public class QueryFilterTest : DatabaseTest
    {
	    [Test]
	    public void AfterBusinessUnitFilterHasBeenTurnedOffOnParameterShouldBeSetAgain()
	    {
		    var dummyBu = new BusinessUnit("_");
				PersistAndRemoveFromUnitOfWork(dummyBu);
		    var scenario = new Scenario("_");
				typeof(VersionedAggregateRootWithBusinessUnit).GetField("_businessUnit", BindingFlags.Instance|BindingFlags.NonPublic).SetValue(scenario, dummyBu);
				PersistAndRemoveFromUnitOfWork(scenario);
		    var rep = new ScenarioRepository(UnitOfWork);

		    using (UnitOfWork.DisableFilter(QueryFilter.BusinessUnit))
		    {
			    rep.LoadAll().Should().Contain(scenario);
		    }
		    rep.LoadAll().Should().Not.Contain(scenario);
	    }

        [Test]
        public void VerifyFilterNamesExistsInMapping()
        {
            Assert.AreEqual("deletedFlagFilter", QueryFilter.Deleted.Name);
            Assert.IsNotNull(Session.SessionFactory.GetFilterDefinition(QueryFilter.Deleted.Name));
        }
    }
}
