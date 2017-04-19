﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture, Category("BucketB")]
	public abstract class WebReportTest : DatabaseTest
    {
        private AnalyticsDataFactory _analyticsDataFactory;
        private IPerson _loggedOnUser;
        private IBusinessUnit _currentBusinessUnit;
        protected int PersonId;
        protected int AcdLoginId;
        protected int ScenarioId;
        protected SpecificDate TheDate;
	    protected SpecificDate Yesterday;
	    protected SpecificDate Tomorrow;

        protected override void SetupForRepositoryTest()
        {
            DataSourceHelper.ClearAnalyticsData();
            _analyticsDataFactory = new AnalyticsDataFactory();
            insertCommonData();
        }

        private void insertCommonData()
        {
            var timeZones = new UtcAndCetTimeZones();
            TheDate = new SpecificDate
            {
                Date = new DateOnly(2014, 2, 7),
                DateId = 1
            };
            Yesterday = new SpecificDate
            {
                Date = new DateOnly(2014, 2, 6),
                DateId = 0
            };
            Tomorrow = new SpecificDate
            {
                Date = new DateOnly(2014, 2, 8),
                DateId = 2
            };
            var intervals = new QuarterOfAnHourInterval();
            var datasource = new ExistingDatasources(timeZones);

            _loggedOnUser = new Domain.Common.Person();
            _loggedOnUser.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
            PersistAndRemoveFromUnitOfWork(_loggedOnUser);

            _currentBusinessUnit = new Domain.Common.BusinessUnit("for test");
            PersistAndRemoveFromUnitOfWork(_currentBusinessUnit);

            PersonId = 76;
            AcdLoginId = 123;
            ScenarioId = 12;

            var agent = new Person(_loggedOnUser, datasource, PersonId, new DateTime(2010, 1, 1),
						 AnalyticsDate.Eternity.DateDate, Yesterday.DateId, Tomorrow.DateId, 0, _currentBusinessUnit.Id.Value, false, timeZones.CetTimeZoneId);
            var scenario = Scenario.DefaultScenarioFor(ScenarioId, _currentBusinessUnit.Id.Value);

			_analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
            _analyticsDataFactory.Setup(timeZones);
			_analyticsDataFactory.Setup(Yesterday);
			_analyticsDataFactory.Setup(TheDate);
			_analyticsDataFactory.Setup(Tomorrow);
            _analyticsDataFactory.Setup(intervals);
            _analyticsDataFactory.Setup(datasource);
            _analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(TheDate, intervals, timeZones, datasource));
            _analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(Yesterday, intervals, timeZones, datasource));
            _analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(Tomorrow, intervals, timeZones, datasource));
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
                var globalSettingRep = new GlobalSettingDataRepository(CurrUnitOfWork);
                var adherenceSetting = globalSettingRep.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
                adherenceSetting.CalculationMethod = AdherenceSetting.Value;
                globalSettingRep.PersistSettingValue(adherenceSetting);
                UnitOfWork.Flush();
            }
        }

        protected abstract void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory);

        protected T Target<T>(Func<ILoggedOnUser, ICurrentDataSource, ICurrentBusinessUnit, IGlobalSettingDataRepository, T> createTarget)
        {
			var currentBusinessUnit = new FakeCurrentBusinessUnit();
	        currentBusinessUnit.FakeBusinessUnit(_currentBusinessUnit);

			return createTarget(new FakeLoggedOnUser(_loggedOnUser),
                CurrentDataSource.Make(),
				currentBusinessUnit, 
				new GlobalSettingDataRepository(CurrUnitOfWork));
        }

        protected virtual AdherenceReportSettingCalculationMethod? AdherenceSetting
        {
            get { return null; }
        }
    }
}