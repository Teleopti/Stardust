﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsScenarioRepositoryTest
	{
		public IAnalyticsScenarioRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldLoadScenarios()
		{
			analyticsDataFactory.Setup(new Scenario(10, Guid.NewGuid(), false));
			analyticsDataFactory.Setup(new Scenario(1, Guid.NewGuid(), true));
			analyticsDataFactory.Persist();

			var scenarios = WithAnalyticsUnitOfWork.Get(() => Target.Scenarios());
			scenarios.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldAddScenarioAndMapAllValues()
		{
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = Guid.NewGuid(),
				ScenarioName = "Scenario Name",
				BusinessUnitId = -1,
				BusinessUnitCode = Guid.NewGuid(),
				BusinessUnitName = "BU",
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DefaultScenario = true,
				IsDeleted = true
			};

			WithAnalyticsUnitOfWork.Do(() => Target.AddScenario(analyticsScenario));
			WithAnalyticsUnitOfWork.Do(() => Target.Scenarios().Count.Should().Be.EqualTo(1));
			var scenario = WithAnalyticsUnitOfWork.Get(() => Target.Scenarios().First(a => a.ScenarioCode == analyticsScenario.ScenarioCode));
			scenario.ScenarioCode.Should().Be.EqualTo(analyticsScenario.ScenarioCode);
			scenario.ScenarioName.Should().Be.EqualTo(analyticsScenario.ScenarioName);
			scenario.BusinessUnitId.Should().Be.EqualTo(analyticsScenario.BusinessUnitId);
			scenario.BusinessUnitCode.Should().Be.EqualTo(analyticsScenario.BusinessUnitCode);
			scenario.BusinessUnitName.Should().Be.EqualTo(analyticsScenario.BusinessUnitName);
			scenario.DatasourceId.Should().Be.EqualTo(analyticsScenario.DatasourceId);
			scenario.DatasourceUpdateDate.Should().Be.EqualTo(analyticsScenario.DatasourceUpdateDate);
			scenario.DefaultScenario.Should().Be.EqualTo(analyticsScenario.DefaultScenario);
			scenario.IsDeleted.Should().Be.EqualTo(analyticsScenario.IsDeleted);
		}

		[Test]
		public void ShouldUpdateScenarioAndMapAllValues()
		{
			var businessUnitCode = Guid.NewGuid();
			var analyticsScenarioSetup = new Scenario(1, businessUnitCode, true);
			analyticsDataFactory.Setup(analyticsScenarioSetup);
			analyticsDataFactory.Persist();
			WithAnalyticsUnitOfWork.Do(() => Target.Scenarios().Count.Should().Be.EqualTo(1));

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = analyticsScenarioSetup.ScenarioCode,
				ScenarioName = "Scenario Name",
				BusinessUnitId = -1,
				BusinessUnitCode = businessUnitCode,
				BusinessUnitName = "BU",
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1),
				DefaultScenario = false,
				IsDeleted = false
			};

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateScenario(analyticsScenario));
			WithAnalyticsUnitOfWork.Do(() => Target.Scenarios().Count.Should().Be.EqualTo(1));
			var scenario = WithAnalyticsUnitOfWork.Get(() => Target.Scenarios().First(a => a.ScenarioCode == analyticsScenario.ScenarioCode));
			scenario.ScenarioCode.Should().Be.EqualTo(analyticsScenario.ScenarioCode);
			scenario.ScenarioName.Should().Be.EqualTo(analyticsScenario.ScenarioName);
			scenario.BusinessUnitId.Should().Be.EqualTo(analyticsScenario.BusinessUnitId);
			scenario.BusinessUnitCode.Should().Be.EqualTo(analyticsScenario.BusinessUnitCode);
			scenario.BusinessUnitName.Should().Be.EqualTo(analyticsScenario.BusinessUnitName);
			scenario.DatasourceId.Should().Be.EqualTo(analyticsScenario.DatasourceId);
			scenario.DatasourceUpdateDate.Should().Be.EqualTo(analyticsScenario.DatasourceUpdateDate);
			scenario.DefaultScenario.Should().Be.EqualTo(analyticsScenario.DefaultScenario);
			scenario.IsDeleted.Should().Be.EqualTo(analyticsScenario.IsDeleted);
		}
	}
}