using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
	[DatabaseTest]
    public class QueryFilterTest
    {
	    public IBusinessUnitRepository BusinessUnits;
	    public IScenarioRepository Scenarios;
	    public WithUnitOfWork WithUnitOfWork;

	    [Test]
	    public void AfterBusinessUnitFilterHasBeenTurnedOffOnParameterShouldBeSetAgain()
	    {
			var dummyBu = new BusinessUnit("_");
			
			WithUnitOfWork.Do(() =>
			{
				BusinessUnits.Add(dummyBu);
				
			});
			var scenario = new Scenario("_");
			typeof(AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scenario, dummyBu);
			WithUnitOfWork.Do(() =>
			{
				Scenarios.Add(scenario);
			});
		    WithUnitOfWork.Do(uow =>
		    {
				using (uow.Current().DisableFilter(QueryFilter.BusinessUnit))
					Scenarios.LoadAll().Should().Contain(scenario);
			});
			WithUnitOfWork.Do(uow =>
			{
					Scenarios.LoadAll().Should().Not.Contain(scenario);
			});
	    }

		[Test]
		public void VerifyFilterNamesExistsInMapping()
		{
			Assert.AreEqual("deletedFlagFilter", QueryFilter.Deleted.Name);
			WithUnitOfWork.Do(uow =>
			{
				Assert.IsNotNull(uow.Current().FetchSession().SessionFactory.GetFilterDefinition(QueryFilter.Deleted.Name));
			});
			
		}
	}
}
