using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class TimeZoneTransformerTest
    {
        private TimeZoneTransformer _target;
        private TimeZoneInfo _defaultTimeZone;
        private IList<TimeZoneDim> _timeZoneDimList;
        private IList<TimeZoneBridge> _timeZoneBridgeList;
        private DateTime _insertDateTime;
        private TimeZoneInfo _timeZoneWEuropeStTime;
        private DataRow _rowDim0;
        private DataRow _rowDim1;
        private DataRow _rowDimMax;
        private DataRow _rowBridge0;
        private DataRow _rowBridgeMax;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _insertDateTime = DateTime.Now;
            _timeZoneWEuropeStTime = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
            IList<TimeZoneInfo> timeZoneCollection = new List<TimeZoneInfo>();
            timeZoneCollection.Add(_timeZoneWEuropeStTime);
            timeZoneCollection.Add(_defaultTimeZone);
            _timeZoneDimList = new TimeZoneDimFactory().Create(_defaultTimeZone, timeZoneCollection, new List<TimeZoneInfo>());
            
            var period =
                new DateTimePeriod(new DateTime(2006, 3, 25, 10, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2006, 3, 26, 14, 0, 0, DateTimeKind.Utc));
            var timeZonePeriodList = new List<TimeZonePeriod>();
            timeZonePeriodList.Add(new TimeZonePeriod { TimeZoneCode = _timeZoneWEuropeStTime.Id, PeriodToLoad = period });
            timeZonePeriodList.Add(new TimeZonePeriod { TimeZoneCode = _defaultTimeZone.Id, PeriodToLoad = period });

            _timeZoneBridgeList = TimeZoneBridgeFactory.CreateTimeZoneBridgeList(timeZonePeriodList, 96);

            _target = new TimeZoneTransformer(_insertDateTime);

            using (DataTable dimDataTable = new DataTable())
            {
                dimDataTable.Locale = Thread.CurrentThread.CurrentCulture;
                TimeZoneInfrastructure.AddColumnsToDimensionDataTable(dimDataTable);

                _target.TransformDim(_timeZoneDimList, dimDataTable);

                Assert.AreEqual(_timeZoneDimList.Count, dimDataTable.Rows.Count);
                _rowDim0 = dimDataTable.Rows[0];
                _rowDim1 = dimDataTable.Rows[1];
                _rowDimMax = dimDataTable.Rows[dimDataTable.Rows.Count - 1];
            }

            using (DataTable bridgeDataTable = new DataTable())
            {
                bridgeDataTable.Locale = Thread.CurrentThread.CurrentCulture;
                TimeZoneInfrastructure.AddColumnsToBridgeDataTable(bridgeDataTable);

                _target.TransformBridge(_timeZoneBridgeList, bridgeDataTable);

                Assert.AreEqual(_timeZoneBridgeList.Count, bridgeDataTable.Rows.Count);
                _rowBridge0 = bridgeDataTable.Rows[0];
                _rowBridgeMax = bridgeDataTable.Rows[bridgeDataTable.Rows.Count - 1];
            }


            //TODO: Create tests for periods over daylight savings
        }

        #endregion

        [Test]
        public void VerifyDefaultTimeZone()
        {
            TimeZoneDim expectedTimeZone = null;
            foreach (TimeZoneDim timeZoneDim in _timeZoneDimList)
            {
                if (timeZoneDim.TimeZoneCode == _defaultTimeZone.Id)
                {
                    expectedTimeZone = timeZoneDim;
                    break;
                }
            }
            Assert.IsNotNull(expectedTimeZone);
            Assert.IsTrue(expectedTimeZone.IsDefaultTimeZone);
        }

        [Test]
        public void VerifyTimeZoneBridgeCreation()
        {
            DateTime firstIntervalDate = new DateTime(2006, 3, 24, 23, 0, 0, DateTimeKind.Utc);
            DateTime lastIntervalDate = new DateTime(2006, 3, 27, 22, 00, 0, DateTimeKind.Utc);
            TimeZoneBridge tzbFirstInterval = new TimeZoneBridge(firstIntervalDate, _timeZoneWEuropeStTime, 96);
            TimeZoneBridge tzbLastInterval = new TimeZoneBridge(lastIntervalDate, _timeZoneWEuropeStTime, 96);

            Assert.AreEqual(firstIntervalDate.Date, tzbFirstInterval.Date);
            Assert.AreEqual(new Interval(firstIntervalDate, 96).Id, tzbFirstInterval.IntervalId);
            Assert.AreEqual(_timeZoneWEuropeStTime.Id, tzbFirstInterval.TimeZoneCode);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeFromUtc(firstIntervalDate, _timeZoneWEuropeStTime).Date,
                            tzbFirstInterval.LocalDate);
            Assert.AreEqual(
                new Interval(TimeZoneInfo.ConvertTimeFromUtc(firstIntervalDate, _timeZoneWEuropeStTime), 96).Id,
                tzbFirstInterval.LocalIntervalId);

            Assert.AreEqual(lastIntervalDate.Date, tzbLastInterval.Date);
            Assert.AreEqual(new Interval(lastIntervalDate, 96).Id, tzbLastInterval.IntervalId);
            Assert.AreEqual(_timeZoneWEuropeStTime.Id, tzbLastInterval.TimeZoneCode);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeFromUtc(lastIntervalDate, _timeZoneWEuropeStTime).Date,
                            tzbLastInterval.LocalDate);
            Assert.AreEqual(
                new Interval(TimeZoneInfo.ConvertTimeFromUtc(lastIntervalDate, _timeZoneWEuropeStTime), 96).Id,
                tzbLastInterval.LocalIntervalId);
        }

        [Test]
        public void VerifyTimeZoneBridgeDataTable()
        {
            
            Assert.AreEqual(_timeZoneBridgeList[0].Date, _rowBridge0["date"]);
            Assert.AreEqual(_timeZoneBridgeList[_timeZoneBridgeList.Count - 1].Date, _rowBridgeMax["date"]);
            Assert.AreEqual(_timeZoneBridgeList[0].IntervalId, _rowBridge0["interval_id"]);
            Assert.AreEqual(_timeZoneBridgeList[_timeZoneBridgeList.Count - 1].IntervalId, _rowBridgeMax["interval_id"]);
            Assert.AreEqual(_timeZoneBridgeList[0].TimeZoneCode, _rowBridge0["time_zone_code"]);
            Assert.AreEqual(_timeZoneBridgeList[_timeZoneBridgeList.Count - 1].TimeZoneCode, _rowBridgeMax["time_zone_code"]);
            Assert.AreEqual(_timeZoneBridgeList[0].LocalDate, _rowBridge0["local_date"]);
            Assert.AreEqual(_timeZoneBridgeList[_timeZoneBridgeList.Count - 1].LocalDate, _rowBridgeMax["local_date"]);
            Assert.AreEqual(_timeZoneBridgeList[0].LocalIntervalId, _rowBridge0["local_interval_id"]);
            Assert.AreEqual(_timeZoneBridgeList[_timeZoneBridgeList.Count - 1].LocalIntervalId, _rowBridgeMax["local_interval_id"]);
        }

        [Test]
        public void VerifyTimeZoneDimCreation()
        {
            var tz = new TimeZoneDim(_timeZoneWEuropeStTime, _timeZoneWEuropeStTime.Id == _defaultTimeZone.Id, false);

            Assert.AreEqual(_timeZoneWEuropeStTime.Id, tz.TimeZoneCode);
            Assert.AreEqual(_timeZoneWEuropeStTime.DisplayName, tz.TimeZoneName);
            Assert.AreEqual(false, tz.IsDefaultTimeZone);
            Assert.AreEqual(Convert.ToInt32(_timeZoneWEuropeStTime.BaseUtcOffset.TotalMinutes), tz.UtcConversion);
            Assert.AreEqual(Convert.ToInt32(_timeZoneWEuropeStTime.BaseUtcOffset.TotalMinutes) + 60, tz.UtcConversionDst);
        }

        [Test]
        public void VerifyTimeZoneDimDataTable()
        {

            Assert.AreEqual(_timeZoneDimList[0].TimeZoneCode, _rowDim0["time_zone_code"]);
            Assert.AreEqual(_timeZoneDimList[_timeZoneDimList.Count - 1].TimeZoneName, _rowDimMax["time_zone_name"]);
            Assert.AreEqual(_timeZoneDimList[1].IsDefaultTimeZone, _rowDim1["default_zone"]);
            Assert.AreEqual(_timeZoneDimList[1].UtcConversion, _rowDim1["utc_conversion"]);
            Assert.AreEqual(_timeZoneDimList[0].UtcConversionDst, _rowDim0["utc_conversion_dst"]);
            Assert.AreEqual(_timeZoneDimList[0].IsUtcInUse, _rowDim0["utc_in_use"]);
            Assert.AreEqual(1, _rowDim0["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _rowDim0["insert_date"]);
            Assert.AreEqual(_insertDateTime, _rowDimMax["update_date"]);
        }
    }
}