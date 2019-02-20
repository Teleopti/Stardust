using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Scenario;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScenarioTests.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsScenarioUpdaterTests : IExtendSystem
	{
		public AnalyticsScenarioUpdater Target;
		public FakeAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsScenarioUpdater>();
		}

		[Test]
		public void ShouldAddScenarioToAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsScenarioRepository.Scenarios().Should().Be.Empty();
			var scenario = ScenarioFactory.CreateScenarioWithId("Test scenario", true);
			ScenarioRepository.Add(scenario);
			Target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsScenarioRepository.Scenarios().First();
			analyticsScenario.DefaultScenario.GetValueOrDefault().Should().Be.True();
			analyticsScenario.ScenarioName.Should().Be.EqualTo("Test scenario");
		}
		
		[Test]
		public void ShouldOnlyAddScenarioReportableToAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsScenarioRepository.Scenarios().Should().Be.Empty();
			var scenario = ScenarioFactory.CreateScenarioWithId("Test scenario", false);
			scenario.EnableReporting = false;
			ScenarioRepository.Add(scenario);
			Target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldUpdateScenarioToAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsScenarioRepository.Scenarios().Should().Be.Empty();
			var scenario = ScenarioFactory.CreateScenarioWithId("New scenario name", true);
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = "Old scenario name",
				DefaultScenario = true
			});
			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);

			Target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsScenarioRepository.Scenarios().First();
			analyticsScenario.DefaultScenario.GetValueOrDefault().Should().Be.True();
			analyticsScenario.ScenarioName.Should().Be.EqualTo("New scenario name");
		}
		
		[Test]
		public void ShouldSetScenarioToDeleteWhenDisableReporting()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsScenarioRepository.Scenarios().Should().Be.Empty();
			var scenario = ScenarioFactory.CreateScenarioWithId("Scenario name", false);
			scenario.EnableReporting = false;
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = "Scenario name"
			});
			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			AnalyticsScenarioRepository.Scenarios().First().IsDeleted.Should().Be.False();
			
			Target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsScenarioRepository.Scenarios().First();
			analyticsScenario.IsDeleted.Should().Be.True();
		}

		[Test]
		public void ShouldSetScenarioToDelete()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsScenarioRepository.Scenarios().Should().Be.Empty();
			var scenario = ScenarioFactory.CreateScenarioWithId("Scenario name", true);
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = "Scenario name"
			});
			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			AnalyticsScenarioRepository.Scenarios().First().IsDeleted.Should().Be.False();
			
			Target.Handle(new ScenarioDeleteEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsScenarioRepository.Scenarios().First();
			analyticsScenario.IsDeleted.Should().Be.True();
		}

	}
}