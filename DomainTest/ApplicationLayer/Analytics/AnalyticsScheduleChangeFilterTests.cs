﻿using NUnit.Framework;
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
			Target.ContinueProcessingEvent(true, reportableScenario.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void DefaultScenarioNotReportable_ShoulBeProcessed()
		{
			addScenarios();
			Target.ContinueProcessingEvent(true, notReportableScenario.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void ScenarioReportable_ShoulBeProcessed()
		{
			addScenarios();
			Target.ContinueProcessingEvent(false, reportableScenario.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void ScenarioNotReportable_ShoulNotBeProcessed()
		{
			addScenarios();
			Target.ContinueProcessingEvent(false, notReportableScenario.Id.GetValueOrDefault()).Should().Be.False();
		}
	}
}
