using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class DimDayOffTests
	{
		private JobParameters jobParameters;
		private const string datasourceName = "Teleopti CCC Agg: Default log object";
		private IUnitOfWork uow;
		private CurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork;
		private ExistingDatasources _datasource;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository = new AnalyticsDayOffRepository(CurrentAnalyticsUnitOfWork.Make());

		private readonly IDayOffTemplateRepository _dayOffRepository = new DayOffTemplateRepository(CurrentUnitOfWork.Make());

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();

			EnsureUnitOfWorks();

			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();

			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			var timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(timeZones);

			analyticsDataFactory.Setup(_datasource);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Persist();

			const string timeZoneId = "W. Europe Standard Time";
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

			var raptorRepository = new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, "");
			jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
			{
				Helper = new JobHelperForTest(raptorRepository, null),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};
		}

		private void EnsureUnitOfWorks()
		{
			var currentAnalyticsUnitOfWorkFactory = CurrentAnalyticsUnitOfWorkFactory.Make();
			uow = currentAnalyticsUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			currentAnalyticsUnitOfWork = new CurrentAnalyticsUnitOfWork(currentAnalyticsUnitOfWorkFactory);
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void JobStepRunAndCreateDayOff()
		{
			var dayOff1 = new DayOffTemplateConfigurable
			{
				Name = "DayOff",
				ShortName = "DO"
			};
			Data.Apply(dayOff1);

			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
			analyticsDataFactory.Setup(new DimDayOff(-1, null, "Not Defined", _datasource, 1));
			analyticsDataFactory.Persist();

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			EnsureUnitOfWorks();
			using (var auow = new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make()).Current())
			{
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffName == dayOff1.Name);
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(0);
				analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOff1.Name);
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOff1.ShortName);
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-8355712);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#808080");
			}
		}

		[Test]
		public void JobStepRunAndCreateNotDefinedDayOff()
		{
			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			EnsureUnitOfWorks();
			using (var auow = new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make()).Current())
			{
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(1);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffId == -1);
				analyticsDayOff.DayOffCode.HasValue.Should().Be.False();
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(-1);
				analyticsDayOff.DayOffName.Should().Be.EqualTo("Not Defined");
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo("Not Defined");
				analyticsDayOff.DatasourceId.Should().Be.EqualTo(-1);
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-1);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#FFFFFF");
			}
		}

		[Test]
		public void JobStepRunAndUpdateDayOffWithNullCodeInAnalytics()
		{
			var dayOff1 = new DayOffTemplateConfigurable { Name = "Normal Day Off" };
			Data.Apply(dayOff1);

			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
			analyticsDataFactory.Setup(new DimDayOff(123, null, "Normal Day Off", _datasource, 0));
			analyticsDataFactory.Persist();

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			EnsureUnitOfWorks();
			using (var auow = new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make()).Current())
			{
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffName == dayOff1.Name);
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(0);
				analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOff1.Name);
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo("");
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-8355712);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#808080");
				analyticsDayOff.DayOffCode.GetValueOrDefault().Should().Not.Be.Null();
			}
		}

		[Test]
		public void JobStepRunAndUpdateDayOffWithCodeInAnalytics()
		{
			var dayOff1 = new DayOffTemplateConfigurable
			{
				Name = "Normal Day Off",
				ShortName = "DO"
			};
			Data.Apply(dayOff1);


			var id = _dayOffRepository.FindAllDayOffsSortByDescription().First().Id.GetValueOrDefault();

			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
			analyticsDataFactory.Setup(new DimDayOff(123, id, "Normal Day Off", _datasource, 0));
			analyticsDataFactory.Persist();

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			EnsureUnitOfWorks();
			using (var auow = new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make()).Current())
			{
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffCode == id);
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(0);
				analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOff1.Name);
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo("DO");
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-8355712);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#808080");
				analyticsDayOff.DayOffCode.GetValueOrDefault().Should().Not.Be.Null();
			}
		}

		[Test]
		public void JobStepRuns()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
			analyticsDataFactory.Persist();

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			var dayOff1 = new DayOffTemplateConfigurable
			{
				Name = "Normal Day Off",
				ShortName = "DO"
			};
			Data.Apply(dayOff1);

			EnsureUnitOfWorks();
			var result2 = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result2.HasError.Should().Be.False();

			EnsureUnitOfWorks();
			using (var auow = new CurrentAnalyticsUnitOfWork(CurrentAnalyticsUnitOfWorkFactory.Make()).Current())
			{
				var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffId == -1);
				analyticsDayOff.DayOffCode.HasValue.Should().Be.False();
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(-1);
				analyticsDayOff.DayOffName.Should().Be.EqualTo("Not Defined");
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo("Not Defined");
				analyticsDayOff.DatasourceId.Should().Be.EqualTo(-1);
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-1);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#FFFFFF");
			}
		}
	}
}