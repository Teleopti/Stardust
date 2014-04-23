using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public abstract class WebReportTest : DatabaseTest
	{
		private AnalyticsDataFactory _analyticsDataFactory;
		private IPerson _loggedOnUser;
		private ExistingDatasources _datasource;
		private IBusinessUnit _currentBusinessUnit;
		protected int PersonId;
		protected int AcdLoginId;
		protected int ScenarioId;
		protected TodayDate Today;

		protected override void SetupForRepositoryTest()
		{
			DataSourceHelper.ClearAnalyticsData();
			_analyticsDataFactory = new AnalyticsDataFactory();
			insertCommonData();
		}

		private void insertCommonData()
		{
			var timeZones = new UtcAndCetTimeZones();
			Today = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			_datasource = new ExistingDatasources(timeZones);

			_loggedOnUser = new Domain.Common.Person();
			_loggedOnUser.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersistAndRemoveFromUnitOfWork(_loggedOnUser);

			_currentBusinessUnit = new Domain.Common.BusinessUnit("for test");
			PersistAndRemoveFromUnitOfWork(_currentBusinessUnit);

			PersonId = 76;
			AcdLoginId = 123;
			ScenarioId = 12;

			var agent = new Person(_loggedOnUser, _datasource, PersonId, new DateTime(2010, 1, 1),
						 new DateTime(2059, 12, 31), 0, -2, 0, _currentBusinessUnit.Id.Value, false, timeZones.CetTimeZoneId);
			var scenario = Scenario.DefaultScenarioFor(ScenarioId, _currentBusinessUnit.Id.Value);

			_analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			_analyticsDataFactory.Setup(timeZones);
			_analyticsDataFactory.Setup(Today);
			_analyticsDataFactory.Setup(intervals);
			_analyticsDataFactory.Setup(_datasource);
			_analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(Today, intervals, timeZones, _datasource));
			_analyticsDataFactory.Setup(agent);
			_analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(agent, AcdLoginId));
			_analyticsDataFactory.Setup(scenario);

			persistAdherenceSetting();

			InsertTestSpecificData(_analyticsDataFactory);
			_analyticsDataFactory.Persist();
		}

		private void persistAdherenceSetting()
		{
			if (AdherenceSetting.HasValue)
			{
				var globalSettingRep = new GlobalSettingDataRepository(UnitOfWork);
				var adherenceSetting = new GlobalSettingDataRepository(UnitOfWork).FindValueByKey(AdherenceReportSetting.Key,
					new AdherenceReportSetting());
				adherenceSetting.CalculationMethod = AdherenceSetting.Value;
				globalSettingRep.PersistSettingValue(adherenceSetting);
				UnitOfWork.Flush();
			}
		}

		protected abstract void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory);

		protected T Target<T>(Func<ILoggedOnUser, ICurrentDataSource, ICurrentBusinessUnit, IGlobalSettingDataRepository ,T> createTarget )
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(_loggedOnUser);

			var currentBu = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			currentBu.Expect(x => x.Current()).Return(_currentBusinessUnit);


			return createTarget(loggedOnUser, 
				new CurrentDataSource(new CurrentIdentity()),	
				currentBu,
				new GlobalSettingDataRepository(new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()))));
		}

		protected virtual AdherenceReportSettingCalculationMethod? AdherenceSetting
		{
			get { return null; }
		}
	}
}