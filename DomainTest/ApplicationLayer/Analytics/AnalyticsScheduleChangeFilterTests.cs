using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Analytics
{
	[DomainTest]
	[TestFixture]
	public class AnalyticsScheduleChangeFilterTests : ISetup
	{
		public AnalyticsScheduleChangeForAllReportableScenariosFilter Target;
		public IScenarioRepository Scenarios;
		private Scenario reportableScenario;
		private Scenario notReportableScenario;
		private Scenario defaultScenario;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsScheduleChangeForAllReportableScenariosFilter>();
		}

		private void addScenarios()
		{
			defaultScenario = new Scenario("Default")
			{
				DefaultScenario = true,
				EnableReporting = false
			}.WithId();

			reportableScenario = new Scenario("Reportable")
			{
				DefaultScenario = false,
				EnableReporting = true
			}.WithId();

			notReportableScenario = new Scenario("Not reportable")
			{
				DefaultScenario = false,
				EnableReporting = false
			}.WithId();
			Scenarios.Add(reportableScenario);
			Scenarios.Add(notReportableScenario);
			Scenarios.Add(defaultScenario);
		}

		[Test]
		public void DefaultScenarioReportable_ShoulBeProcessed()
		{
			addScenarios();

			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				ScenarioId = defaultScenario.Id.GetValueOrDefault()
			};
			Target.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void DefaultScenarioNotReportable_ShoulBeProcessed()
		{
			addScenarios();

			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				ScenarioId = defaultScenario.Id.GetValueOrDefault()
			};
			Target.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void ScenarioReportable_ShoulBeProcessed()
		{
			addScenarios();

			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
				ScenarioId = reportableScenario.Id.GetValueOrDefault()
			};
			Target.ContinueProcessingEvent(@event).Should().Be.True();
		}

		[Test]
		public void ScenarioNotReportable_ShoulNotBeProcessed()
		{
			addScenarios();

			var @event = new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
				ScenarioId = notReportableScenario.Id.GetValueOrDefault()
			};
			Target.ContinueProcessingEvent(@event).Should().Be.False();
		}
	}
}
