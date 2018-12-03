using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class DateTransformerTest
    {
        private DataTable _dataTable;
        private IList<DayDate> _dateList;
        private IList<DayDate> _dateList1;
        private IList<DayDate> _dateList2;
        private IList<DayDate> _dateList3;
        private readonly DateTime _insertDateTime = DateTime.Now;
        private DateTransformer _target;
        private readonly CultureInfo _swedishCulture = new CultureInfo("sv-SE");


        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _dateList1 = DateFactory.CreateDateCollection(_swedishCulture);
            _dateList2 = DateFactory.CreateDateCollection(new CultureInfo("en-GB"));
            _dateList3 = DateFactory.CreateDateCollection(new CultureInfo("en-US"));

            _dataTable = new DataTable();
            _dataTable.Locale = Thread.CurrentThread.CurrentCulture;
            DateInfrastructure.AddColumnsToDataTable(_dataTable);

            var period = new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2007, 1, 5, 0, 0, 0, DateTimeKind.Utc));
            _target = new DateTransformer(_insertDateTime);
            _dateList = DateTransformer.CreateDateList(period);
            _target.Transform(_dateList, _dataTable);
        }

        #endregion

        // Public Methods (7) 
        [Test]
        public void VerifyMonth()
        {
            // Today it is not sure how we will handle month and week names. Maybe it will be made
            // localized from the data amrt instead. Will see...
            Assert.AreEqual(2007, _dateList2[0].Year);
            Assert.AreEqual("200705", _dateList1[1].YearMonth);
            Assert.AreEqual("May", _dateList2[1].MonthName);
            Assert.AreEqual("januari", _dateList1[0].MonthName);
            Assert.AreEqual(1, _dateList1[0].DayInMonth);
            Assert.AreEqual(31, _dateList3[2].DayInMonth);
        }

        [Test]
        public void VerifyQuarter()
        {
            Assert.AreEqual("2007Q1", _dateList1[0].Quarter);
            Assert.AreEqual("2007Q2", _dateList1[1].Quarter);
            Assert.AreEqual("2006Q3", _dateList1[5].Quarter);
            Assert.AreEqual("2006Q4", _dateList1[2].Quarter);
        }

        [Test]
        public void VerifyWeek()
        {
            // Today it is not sure how we will handle month and week names. Maybe it will be made
            // localized from the data amrt instead. Will see...
            Assert.AreEqual(1, _dateList1[0].WeekdayNumber); //SE
            Assert.AreEqual(7, _dateList3[2].WeekdayNumber); //US

            Assert.AreEqual("Monday", _dateList2[0].WeekdayName); //GB
            Assert.AreEqual("söndag", _dateList1[2].WeekdayName); //SE

            Assert.AreEqual(1, _dateList1[0].WeekNumber); //SE
            Assert.AreEqual(52, _dateList1[2].WeekNumber); //SE
            Assert.AreEqual(1, _dateList2[0].WeekNumber); //GB
            Assert.AreEqual(52, _dateList2[2].WeekNumber); //GB
            Assert.AreEqual(1, _dateList3[0].WeekNumber); //US
            Assert.AreEqual(53, _dateList3[2].WeekNumber); //US

            Assert.AreEqual("200701", _dateList1[0].YearWeek); //SE
            Assert.AreEqual("200652", _dateList1[2].YearWeek); //SE
            Assert.AreEqual("200652", _dateList2[2].YearWeek); //GB
        }

        [Test]
        public void VerifyDate()
        {
            var date1 = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2007, 5, 31, 0, 0, 0, DateTimeKind.Utc);
            var date3 = new DateTime(2006, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(date1, _dateList1[0].DateDate);
            Assert.AreEqual(date2, _dateList2[1].DateDate);
            Assert.AreEqual(date3, _dateList3[2].DateDate);
        }

        [Test]
        public void VerifyMonthResourceKey()
        {
            var dayDateMonth1 = new DayDate(new DateTime(2009, 1, 1), _swedishCulture);
            var dayDateMonth2 = new DayDate(new DateTime(2009, 2, 1), _swedishCulture);
            var dayDateMonth3 = new DayDate(new DateTime(2009, 3, 1), _swedishCulture);
            var dayDateMonth4 = new DayDate(new DateTime(2009, 4, 1), _swedishCulture);
            var dayDateMonth5 = new DayDate(new DateTime(2009, 5, 1), _swedishCulture);
            var dayDateMonth6 = new DayDate(new DateTime(2009, 6, 1), _swedishCulture);
            var dayDateMonth7 = new DayDate(new DateTime(2009, 7, 1), _swedishCulture);
            var dayDateMonth8 = new DayDate(new DateTime(2009, 8, 1), _swedishCulture);
            var dayDateMonth9 = new DayDate(new DateTime(2009, 9, 1), _swedishCulture);
            var dayDateMonth10 = new DayDate(new DateTime(2009, 10, 1), _swedishCulture);
            var dayDateMonth11 = new DayDate(new DateTime(2009, 11, 1), _swedishCulture);
            var dayDateMonth12 = new DayDate(new DateTime(2009, 12, 1), _swedishCulture);

            Assert.AreEqual("ResMonthJanuary", dayDateMonth1.MonthResourceKey);
            Assert.AreEqual("ResMonthFebruary", dayDateMonth2.MonthResourceKey);
            Assert.AreEqual("ResMonthMarch", dayDateMonth3.MonthResourceKey);
            Assert.AreEqual("ResMonthApril", dayDateMonth4.MonthResourceKey);
            Assert.AreEqual("ResMonthMay", dayDateMonth5.MonthResourceKey);
            Assert.AreEqual("ResMonthJune", dayDateMonth6.MonthResourceKey);
            Assert.AreEqual("ResMonthJuly", dayDateMonth7.MonthResourceKey);
            Assert.AreEqual("ResMonthAugust", dayDateMonth8.MonthResourceKey);
            Assert.AreEqual("ResMonthSeptember", dayDateMonth9.MonthResourceKey);
            Assert.AreEqual("ResMonthOctober", dayDateMonth10.MonthResourceKey);
            Assert.AreEqual("ResMonthNovember", dayDateMonth11.MonthResourceKey);
            Assert.AreEqual("ResMonthDecember", dayDateMonth12.MonthResourceKey);
        }

        [Test]
        public void VerifyWeekdayResourceKey()
        {

            var dayDateWeekDay1 = new DayDate(new DateTime(2009, 1, 5), _swedishCulture);
            var dayDateWeekDay2 = new DayDate(new DateTime(2009, 1, 6), _swedishCulture);
            var dayDateWeekDay3 = new DayDate(new DateTime(2009, 1, 7), _swedishCulture);
            var dayDateWeekDay4 = new DayDate(new DateTime(2009, 1, 8), _swedishCulture);
            var dayDateWeekDay5 = new DayDate(new DateTime(2009, 1, 9), _swedishCulture);
            var dayDateWeekDay6 = new DayDate(new DateTime(2009, 1, 10), _swedishCulture);
            var dayDateWeekDay7 = new DayDate(new DateTime(2009, 1, 11), _swedishCulture);

            Assert.AreEqual("ResDayOfWeekMonday", dayDateWeekDay1.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekTuesday", dayDateWeekDay2.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekWednesday", dayDateWeekDay3.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekThursday", dayDateWeekDay4.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekFriday", dayDateWeekDay5.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekSaturday", dayDateWeekDay6.WeekdayResourceKey);
            Assert.AreEqual("ResDayOfWeekSunday", dayDateWeekDay7.WeekdayResourceKey);
        }

        [Test]
        public void VerifyDateTable()
        {
            Assert.AreEqual(5, _dataTable.Rows.Count);

            Assert.AreEqual(_dateList[0].DateDate, _dataTable.Rows[0]["date_date"]);
            Assert.AreEqual(_dateList[1].Year, _dataTable.Rows[1]["year"]);
            Assert.AreEqual(_dateList[2].YearMonth, _dataTable.Rows[2]["year_month"]);
            Assert.AreEqual(_dateList[3].Month, _dataTable.Rows[3]["month"]);
            Assert.AreEqual(_dateList[4].MonthName, _dataTable.Rows[4]["month_name"]);
            Assert.AreEqual(_dateList[0].MonthResourceKey, _dataTable.Rows[0]["month_resource_key"]);
            Assert.AreEqual(_dateList[0].DayInMonth, _dataTable.Rows[0]["day_in_month"]);
            Assert.AreEqual(_dateList[0].WeekdayNumber, _dataTable.Rows[0]["weekday_number"]);
            Assert.AreEqual(_dateList[0].WeekdayName, _dataTable.Rows[0]["weekday_name"]);
            Assert.AreEqual(_dateList[0].WeekdayResourceKey, _dataTable.Rows[0]["weekday_resource_key"]);
            Assert.AreEqual(_dateList[0].WeekNumber, _dataTable.Rows[0]["week_number"]);
            Assert.AreEqual(_dateList[0].YearWeek, _dataTable.Rows[0]["year_week"]);
            Assert.AreEqual(_dateList[0].Quarter, _dataTable.Rows[0]["quarter"]);
            Assert.AreEqual(_insertDateTime, _dataTable.Rows[0]["insert_date"]);
        }

        [Test]
        public void VerifyNoPublicEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof (DateTransformer)));
            Assert.IsNotNull(_target);
        }

        [Test]
        public void ShouldSetCorrectYearForYearWeekForSwedishCulture()
        {
            var theDayDate1 = new DayDate(new DateTime(2012, 1, 1), _swedishCulture);
            var theDayDate2 = new DayDate(new DateTime(2011, 12, 31), _swedishCulture);
            var theDayDate3 = new DayDate(new DateTime(2012, 1, 2), _swedishCulture);
            var theDayDate4 = new DayDate(new DateTime(2011, 1, 1), _swedishCulture);
            var theDayDate5 = new DayDate(new DateTime(2010, 12, 31), _swedishCulture);
            var theDayDate6 = new DayDate(new DateTime(2010, 1, 1), _swedishCulture);
            var theDayDate7 = new DayDate(new DateTime(2009, 12, 31), _swedishCulture);
            var theDayDate8 = new DayDate(new DateTime(2014, 1, 1), _swedishCulture);
            var theDayDate9 = new DayDate(new DateTime(2013, 12, 31), _swedishCulture);
            
            Assert.AreEqual("201152", theDayDate1.YearWeek);
            Assert.AreEqual("201152", theDayDate2.YearWeek);
            Assert.AreEqual("201201", theDayDate3.YearWeek);
            Assert.AreEqual("201052", theDayDate4.YearWeek);
            Assert.AreEqual("201052", theDayDate5.YearWeek);
            Assert.AreEqual("200953", theDayDate6.YearWeek);
            Assert.AreEqual("200953", theDayDate7.YearWeek);
            Assert.AreEqual("201401", theDayDate8.YearWeek);
            Assert.AreEqual("201401", theDayDate9.YearWeek);
        } 

        [Test]
        public void ShouldSetCorrectYearForYearWeekForUsCultureAndFirstFullWeekRule()
        {
            var culture = new CultureInfo("en-Us"){DateTimeFormat ={CalendarWeekRule = CalendarWeekRule.FirstFullWeek}};
            var theDayDate1 = new DayDate(new DateTime(2012, 1, 1), culture);
            var theDayDate2 = new DayDate(new DateTime(2011, 12, 31), culture);
            var theDayDate3 = new DayDate(new DateTime(2012, 1, 2), culture);
            var theDayDate4 = new DayDate(new DateTime(2011, 1, 1), culture);
            var theDayDate5 = new DayDate(new DateTime(2010, 12, 31), culture);
            var theDayDate6 = new DayDate(new DateTime(2010, 1, 1), culture);
            var theDayDate7 = new DayDate(new DateTime(2009, 12, 31), culture);
            var theDayDate8 = new DayDate(new DateTime(2014, 1, 1), culture);
            var theDayDate9 = new DayDate(new DateTime(2013, 12, 31), culture);

            Assert.AreEqual("201201", theDayDate1.YearWeek);
            Assert.AreEqual("201152", theDayDate2.YearWeek);
            Assert.AreEqual("201201", theDayDate3.YearWeek);
            Assert.AreEqual("201052", theDayDate4.YearWeek);
            Assert.AreEqual("201052", theDayDate5.YearWeek);
            Assert.AreEqual("200952", theDayDate6.YearWeek);
            Assert.AreEqual("200952", theDayDate7.YearWeek);
            Assert.AreEqual("201352", theDayDate8.YearWeek);
            Assert.AreEqual("201352", theDayDate9.YearWeek);
        }

        [Test]
        public void ShouldSetCorrectYearForYearWeekForUsCultureAndFirstFourDayWeekRule()
        {
            var culture = new CultureInfo("en-Us") { DateTimeFormat = { CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek} };
            var theDayDate1 = new DayDate(new DateTime(2012, 1, 1), culture);
            var theDayDate2 = new DayDate(new DateTime(2011, 12, 31), culture);
            var theDayDate3 = new DayDate(new DateTime(2012, 1, 2), culture);
            var theDayDate4 = new DayDate(new DateTime(2011, 1, 1), culture);
            var theDayDate5 = new DayDate(new DateTime(2010, 12, 31), culture);
            var theDayDate6 = new DayDate(new DateTime(2010, 1, 1), culture);
            var theDayDate7 = new DayDate(new DateTime(2009, 12, 31), culture);
            var theDayDate8 = new DayDate(new DateTime(2014, 1, 1), culture);
            var theDayDate9 = new DayDate(new DateTime(2013, 12, 31), culture);

            Assert.AreEqual("201201", theDayDate1.YearWeek);
            Assert.AreEqual("201152", theDayDate2.YearWeek);
            Assert.AreEqual("201201", theDayDate3.YearWeek);
            Assert.AreEqual("201052", theDayDate4.YearWeek);
            Assert.AreEqual("201052", theDayDate5.YearWeek);
            Assert.AreEqual("200952", theDayDate6.YearWeek);
            Assert.AreEqual("200952", theDayDate7.YearWeek);
            Assert.AreEqual("201401", theDayDate8.YearWeek);
            Assert.AreEqual("201401", theDayDate9.YearWeek);
        }
    }
}