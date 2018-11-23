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
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[InfrastructureTest]
	public class DimPersonUpdateMaxDateJobStepTests : IIsolateSystem
	{
		public IAnalyticsPersonPeriodDateFixer AnalyticsPersonPeriodDateFixer;
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public IAnalyticsDateRepository AnalyticsDateRepository;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private JobParameters jobParameters;
		private ExistingDatasources _datasource;
		private UtcAndCetTimeZones _timeZones;

		public void Isolate(IIsolate isolate)
		{
		}

		[Test]
		public void ShouldRunStepWithoutError()
		{
			setup();
			var step = new DimPersonUpdateMaxDateJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();
		}

		[Test]
		public void ShouldRunStepAndUpdateDates()
		{
			setup();
			var validFrom = new DateTime(2010, 1, 1);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(validFrom));
			personPeriod.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithGuid("FirstName", "LastName");
			person.AddPersonPeriod(personPeriod);
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new Person(person, _datasource, 0, validFrom, AnalyticsDate.Eternity.DateDate, 0, -2, 0,
				BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				var analyticsPersonPeriods = AnalyticsPersonPeriodRepository.GetPersonPeriods(person.Id.GetValueOrDefault());
				analyticsPersonPeriods.Count.Should().Be.EqualTo(1);
				var analyticsPeriod = analyticsPersonPeriods.First();

				analyticsPeriod.ValidToDate.Should().Be.EqualTo(AnalyticsDate.Eternity.DateDate);
				analyticsPeriod.ValidToDateId.Should().Be.EqualTo(-2);
				analyticsPeriod.ValidToDateIdMaxDate.Should().Be.EqualTo(-2);
				analyticsPeriod.ValidToDateIdLocal.Should().Be.EqualTo(-2);
			});

			var step = new DimPersonUpdateMaxDateJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();
			result.RowsAffected.Should().Be.EqualTo(1);

			WithAnalyticsUnitOfWork.Do(() =>
			{
				var maxDate = AnalyticsDateRepository.MaxDate();

				var analyticsPersonPeriods = AnalyticsPersonPeriodRepository.GetPersonPeriods(person.Id.GetValueOrDefault());
				analyticsPersonPeriods.Count.Should().Be.EqualTo(1);
				var analyticsPeriod = analyticsPersonPeriods.First();

				analyticsPeriod.ValidToDate.Should().Be.EqualTo(AnalyticsDate.Eternity.DateDate);
				analyticsPeriod.ValidToDateId.Should().Be.EqualTo(-2);
				analyticsPeriod.ValidToDateIdMaxDate.Should().Be.EqualTo(maxDate.DateId-1);
				analyticsPeriod.ValidToDateIdLocal.Should().Be.EqualTo(maxDate.DateId);
			});
		}

		[Test]
		public void ShouldRunStepButNothingToUpdateDate()
		{
			setup();
			var validFrom = new DateTime(2010, 1, 1);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(validFrom));
			personPeriod.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithGuid("FirstName", "LastName");
			person.AddPersonPeriod(personPeriod);
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new Person(person, _datasource, 0, validFrom, new DateTime(2010, 1, 4), 0, 4, 0,
				BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				var analyticsPersonPeriods = AnalyticsPersonPeriodRepository.GetPersonPeriods(person.Id.GetValueOrDefault());
				analyticsPersonPeriods.Count.Should().Be.EqualTo(1);
			});

			var step = new DimPersonUpdateMaxDateJobStep(jobParameters);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, null, false);
			result.HasError.Should().Be.False();
			result.RowsAffected.Should().Be.EqualTo(0);

			WithAnalyticsUnitOfWork.Do(() =>
			{
				var maxDate = AnalyticsDateRepository.MaxDate();
				var analyticsPersonPeriods = AnalyticsPersonPeriodRepository.GetPersonPeriods(person.Id.GetValueOrDefault());
				analyticsPersonPeriods.Count.Should().Be.EqualTo(1);
				var analyticsPeriod = analyticsPersonPeriods.First();
				analyticsPeriod.ValidToDateIdMaxDate.Should().Not.Be.EqualTo(maxDate.DateId - 1);
				analyticsPeriod.ValidToDateIdLocal.Should().Not.Be.EqualTo(maxDate.DateId);
			});
		}

		private void setup()
		{
			SetupFixtureForAssembly.BeginTest();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();

			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
			var sysConfiguration = new SysConfiguration("IntervalLengthMinutes", "15");
			sysConfiguration.AddConfiguration("TimeZoneCode", "UTC");
			sysConfiguration.AddConfiguration("Culture", CultureInfo.CurrentCulture.LCID.ToString());

			analyticsDataFactory.Setup(_datasource);
			analyticsDataFactory.Setup(_timeZones);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(sysConfiguration);
			analyticsDataFactory.Setup(new BusinessUnit(TestState.BusinessUnit, _datasource));
			analyticsDataFactory.Persist();

			const string datasourceName = "Teleopti CCC Agg: Default log object";
			const string timeZoneId = "W. Europe Standard Time";
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

			var raptorRepository = new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null,
				AnalyticsPersonPeriodDateFixer);
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(UnitOfWorkFactory.Current.ConnectionString);
			var dsFactory = DataSourceHelper.MakeLegacyWay().Make();
			jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
			)
			{
				Helper = new JobHelperForTest(raptorRepository,
					null,
					new Tenants(tenantUnitOfWorkManager,
						new LoadAllTenants(tenantUnitOfWorkManager),
						dsFactory,
						new BaseConfigurationRepository(),
						new TrueToggleManager())),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

			var dataSource = SetupFixtureForAssembly.DataSource;
			jobParameters.Helper.SelectDataSourceContainer(dataSource.DataSourceName);
		}
	}
}