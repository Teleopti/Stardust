using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetGroupMonthFilterTest
    {
        private BudgetGroupMonthFilter target;

        [SetUp]
        public void Setup()
        {
            var budgetGroup = new BudgetGroup
                                  {
                                      Name = "BG",
                                      TimeZone = (TimeZoneInfo.GetSystemTimeZones()[7])
                                  };
            budgetGroup.TrySetDaysPerYear(365);
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var startDay = new DateOnly(2010, 1, 5);

            var list = new List<IBudgetGroupDayDetailModel>();

            for (var i = 0; i < 90; i++)
            {
                list.Add(new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, startDay.AddDays(i))));
            }

            var culture = CultureInfo.GetCultureInfo(1053); //Swedish
            target = new BudgetGroupMonthFilter(list, culture);
        }

        [Test]
        public void ShouldFilterToMonths()
        {

            var filteredList = target.Filter();

            //First Month
            var firstDateList = filteredList.First();
            Assert.AreEqual(28, firstDateList.Count);

            var firstMonthsFirstDay = new DateOnly(2010, 2, 1);
            var i = 0;
            foreach (var detailModel in firstDateList)
            {
                Assert.AreEqual(firstMonthsFirstDay.AddDays(i), detailModel.BudgetDay.Day);
                i++;
            }

            //Second Month
            var secondDateList = filteredList.Last();
            Assert.AreEqual(31, secondDateList.Count);

            var secondMonthsFirstDay = new DateOnly(2010, 3, 1);
            var j = 0;
            foreach (var detailModel in secondDateList)
            {
                Assert.AreEqual(secondMonthsFirstDay.AddDays(j), detailModel.BudgetDay.Day);
                j++;
            }
        }
    }
}
