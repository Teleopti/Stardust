using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
    public static class AnalyticsRunner
    {
        public static void RunAnalyticsBaseData()
        {
            var analyticsDataFactory = new AnalyticsDataFactory();
            var timeZones = new UtcAndCetTimeZones();
            var dates = new CurrentBeforeAndAfterWeekDates();
            var dataSource = new ExistingDatasources(timeZones);
            var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource);
            var intervals = new QuarterOfAnHourInterval();

            analyticsDataFactory.Setup(timeZones);
            analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
            analyticsDataFactory.Setup(dates);
            analyticsDataFactory.Setup(businessUnit);
            analyticsDataFactory.Setup(intervals);
            analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));

            analyticsDataFactory.Persist();
        }  
    }
}