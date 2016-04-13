using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Analytics
{
	[TestFixture]
	public class AnalyticsScheduleChangeFilterTests
	{
		private AnalyticsScheduleChangeForAllReportableScenariosFilter filter;
		private IScenarioRepository scenarioRepository;
		private Scenario reportableScenario;
		private Scenario notReportableScenario;
		private Scenario defaultScenario;

		[SetUp]
		public void AnalyticsScheduleChangeFilterTestsSetup()
		{
			defaultScenario = new Scenario("Default");
			defaultScenario.WithId();
			defaultScenario.DefaultScenario = true;
			defaultScenario.EnableReporting = false;

			reportableScenario = new Scenario("Reportable");
			reportableScenario.WithId();
			reportableScenario.DefaultScenario = false;
			reportableScenario.EnableReporting = true;

			notReportableScenario = new Scenario("Not reportable");
			notReportableScenario.WithId();
			notReportableScenario.DefaultScenario = false;
			notReportableScenario.EnableReporting = false;

			scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Add(reportableScenario);
			scenarioRepository.Add(notReportableScenario);

			filter = new AnalyticsScheduleChangeForAllReportableScenariosFilter(scenarioRepository);
		}

		[Test]
		public void DefaultScenarioReportable_ShoulBeProcessed()
		{
			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				ScenarioId = defaultScenario.Id.GetValueOrDefault()
			};
			filter.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void DefaultScenarioNotReportable_ShoulBeProcessed()
		{
			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				ScenarioId = defaultScenario.Id.GetValueOrDefault()
			};
			filter.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void ScenarioReportable_ShoulBeProcessed()
		{
			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
				ScenarioId = reportableScenario.Id.GetValueOrDefault()
			};
			filter.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void ScenarioNotReportable_ShoulNotBeProcessed()
		{
			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
				ScenarioId = notReportableScenario.Id.GetValueOrDefault()
			};
			filter.ContinueProcessingEvent(@event).Should().Be.False();
		}
	}
}
