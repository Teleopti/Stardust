using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[InfrastructureTest]
	public class DimDayOffTests : IIsolateSystem
	{
		private JobParameters jobParameters;
		private ExistingDatasources _datasource;
		public IAnalyticsDayOffRepository AnalyticsDayOffRepository;
		public IDayOffTemplateRepository DayOffRepository;

		public void Isolate(IIsolate isolate)
		{
		}

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();

			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();

			var eternityAndNotDefinedDate = new EternityAndNotDefinedDate();
			var timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(timeZones);
			var sysConfiguration = new SysConfiguration("IntervalLengthMinutes", "15");
			sysConfiguration.AddConfiguration("TimeZoneCode", "UTC");
			sysConfiguration.AddConfiguration("Culture", CultureInfo.CurrentCulture.LCID.ToString());

			analyticsDataFactory.Setup(eternityAndNotDefinedDate);
			analyticsDataFactory.Setup(_datasource);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(sysConfiguration);
			analyticsDataFactory.Persist();

			const string timeZoneId = "W. Europe Standard Time";
			const string datasourceName = "Teleopti CCC Agg: Default log object";
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

			var raptorRepository = new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null);
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
			var dsFactory = DataSourceHelper.MakeLegacyWay().Make();
			jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
			)
			{
				Helper = new JobHelperForTest(raptorRepository, null,
					new Tenants(tenantUnitOfWorkManager, new LoadAllTenants(tenantUnitOfWorkManager), dsFactory,
						new BaseConfigurationRepository())),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

			var dataSource = SetupFixtureForAssembly.DataSource;
			jobParameters.Helper.SelectDataSourceContainer(dataSource.DataSourceName);
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
			analyticsDataFactory.Setup(new DimDayOff(-1, Guid.Empty, "Not Defined", _datasource, 1));
			analyticsDataFactory.Persist();

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			using (SetupFixtureForAssembly.DataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var analyticsDayOffs = AnalyticsDayOffRepository.DayOffs();
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

			using (SetupFixtureForAssembly.DataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var analyticsDayOffs = AnalyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(1);
				var notDefinedRow = analyticsDayOffs.First(a => a.DayOffId == -1);
				notDefinedRow.DayOffCode.Should().Be.EqualTo(Guid.Empty);
				notDefinedRow.BusinessUnitId.Should().Be.EqualTo(-1);
				notDefinedRow.DayOffName.Should().Be.EqualTo("Not Defined");
				notDefinedRow.DayOffShortname.Should().Be.EqualTo("Not Defined");
				notDefinedRow.DatasourceId.Should().Be.EqualTo(-1);
				notDefinedRow.DisplayColor.Should().Be.EqualTo(-1);
				notDefinedRow.DisplayColorHtml.Should().Be.EqualTo("#FFFFFF");
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
			Guid id;
			using (SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				Data.Apply(dayOff1);

				id = DayOffRepository.FindAllDayOffsSortByDescription().First().Id.GetValueOrDefault();

				var analyticsDataFactory = new AnalyticsDataFactory();
				analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
				analyticsDataFactory.Setup(new DimDayOff(123, id, "Normal Day Off", _datasource, 0));
				analyticsDataFactory.Persist();
			}

			var step = new DimDayOffJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();

			using (SetupFixtureForAssembly.DataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var analyticsDayOffs = AnalyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var analyticsDayOff = analyticsDayOffs.First(a => a.DayOffCode == id);
				analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(0);
				analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOff1.Name);
				analyticsDayOff.DayOffShortname.Should().Be.EqualTo("DO");
				analyticsDayOff.DisplayColor.Should().Be.EqualTo(-8355712);
				analyticsDayOff.DisplayColorHtml.Should().Be.EqualTo("#808080");
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
			var result2 = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result2.HasError.Should().Be.False();

			using (SetupFixtureForAssembly.DataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var analyticsDayOffs = AnalyticsDayOffRepository.DayOffs();
				analyticsDayOffs.Count.Should().Be.EqualTo(2);
				var notDefinedRow = analyticsDayOffs.First(a => a.DayOffId == -1);
				notDefinedRow.DayOffCode.Should().Be.EqualTo(Guid.Empty);
				notDefinedRow.BusinessUnitId.Should().Be.EqualTo(-1);
				notDefinedRow.DayOffName.Should().Be.EqualTo("Not Defined");
				notDefinedRow.DayOffShortname.Should().Be.EqualTo("Not Defined");
				notDefinedRow.DatasourceId.Should().Be.EqualTo(-1);
				notDefinedRow.DisplayColor.Should().Be.EqualTo(-1);
				notDefinedRow.DisplayColorHtml.Should().Be.EqualTo("#FFFFFF");
			}
		}
	}
}