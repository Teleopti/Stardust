using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Scenario;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScenarioTests.Analytics
{
	[TestFixture]
	public class ScenarioChangedHandlerTests
	{
		private AnalyticsScenarioUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private FakeAnalyticsScenarioRepository _analyticsScenarioRepository;
		private FakeScenarioRepository _scenarioRepository;
		private FakeBusinessUnitRepository _businessUnitRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsScenarioRepository = new FakeAnalyticsScenarioRepository();
			_scenarioRepository = new FakeScenarioRepository();
			_businessUnitRepository = new FakeBusinessUnitRepository();

			_businessUnitRepository.Add(BusinessUnitFactory.BusinessUnitUsedInTest);

			_target = new AnalyticsScenarioUpdater(_analyticsBusinessUnitRepository, _analyticsScenarioRepository, _scenarioRepository, _businessUnitRepository);
		}

		[Test]
		public void ShouldAddScenarioToAnalytics()
		{
			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(0);
			var scenario = ScenarioFactory.CreateScenarioWithId("Test scenario", true);
			_scenarioRepository.Add(scenario);
			_target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = _analyticsScenarioRepository.Scenarios().First();
			analyticsScenario.DefaultScenario.GetValueOrDefault().Should().Be.True();
			analyticsScenario.ScenarioName.Should().Be.EqualTo("Test scenario");
		}

		[Test]
		public void ShouldUpdateScenarioToAnalytics()
		{
			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(0);
			var scenario = ScenarioFactory.CreateScenarioWithId("New scenario name", true);
			_scenarioRepository.Add(scenario);
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = "Old scenario name",
				DefaultScenario = true
			});
			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);

			_target.Handle(new ScenarioChangeEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = _analyticsScenarioRepository.Scenarios().First();
			analyticsScenario.DefaultScenario.GetValueOrDefault().Should().Be.True();
			analyticsScenario.ScenarioName.Should().Be.EqualTo("New scenario name");
		}

		[Test]
		public void ShouldSetScenarioToDelete()
		{
			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(0);
			var scenario = ScenarioFactory.CreateScenarioWithId("Scenario name", true);
			_scenarioRepository.Add(scenario);
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioName = "Scenario name"
			});
			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			_analyticsScenarioRepository.Scenarios().First().IsDeleted.Should().Be.False();
			
			_target.Handle(new ScenarioDeleteEvent
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsScenarioRepository.Scenarios().Count.Should().Be.EqualTo(1);
			var analyticsScenario = _analyticsScenarioRepository.Scenarios().First();
			analyticsScenario.IsDeleted.Should().Be.True();
		}
	}
}