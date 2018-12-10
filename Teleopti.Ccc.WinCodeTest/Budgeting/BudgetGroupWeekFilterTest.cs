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
    public class BudgetGroupWeekFilterTest
    {
        private BudgetGroupWeekFilter target;

        [SetUp]
        public void Setup()
        {
            var budgetGroup = new BudgetGroup();
            budgetGroup.Name = "BG";
            budgetGroup.TimeZone = (TimeZoneInfo.GetSystemTimeZones()[7]);
            budgetGroup.TrySetDaysPerYear(365);
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var startDay = new DateOnly(2010, 5, 9);

            var list = new List<IBudgetGroupDayDetailModel>();

            for (int i = 0; i < 18; i++)
            {
                list.Add(new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, startDay.AddDays(i))));    
            }

            var culture = CultureInfo.GetCultureInfo(1053); //Swedish
            target = new BudgetGroupWeekFilter(list, culture);
        }

        [Test]
        public void ShouldFilterToWeeks()
        {

            var filteredList = target.Filter();

            //First Week
            var firstDateList = filteredList.First();
            Assert.AreEqual(7,firstDateList.Count);

            //Måndag 2010-05-10
            var firstMonday = new DateOnly(2010, 5, 10);
            var i = 0;
            foreach (var detailModel in firstDateList)
            {
                Assert.AreEqual(firstMonday.AddDays(i),detailModel.BudgetDay.Day);
                i++;
            }

            //Second Week
            var secondDateList = filteredList.Last();
            Assert.AreEqual(7, secondDateList.Count);

            //Måndag 2010-05-17
            var secondMonday = new DateOnly(2010, 5, 17);
            var j = 0;
            foreach (var detailModel in secondDateList)
            {
                Assert.AreEqual(secondMonday.AddDays(j), detailModel.BudgetDay.Day);
                j++;
            }
        }
    }
}
