using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class DayOffBackToLegalStateValidatorListCreatorTest
    {

        private DayOffBackToLegalStateValidatorListCreator _target;
        private IDaysOffPreferences _daysOffPreferences;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodIndexRange;


        [SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));
            _periodIndexRange = new MinMax<int>(13, 19);
        }

        #region Build Active Validators tests

        [Test]
        public void VerifyConsecutiveDaysOffValidatorCreation()
        {
            _daysOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _daysOffPreferences.UseConsecutiveDaysOff = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof (ConsecutiveDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyConsecutiveWorkdaysValidatorCreation()
        {
            _daysOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _daysOffPreferences.UseConsecutiveWorkdays = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(ConsecutiveWorkdayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyDaysOffPerWeekValidatorCreation()
        {
            _daysOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _daysOffPreferences.UseDaysOffPerWeek = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(WeeklyDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendDayValidatorCreation()
        {
            _daysOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _daysOffPreferences.UseWeekEndDaysOff = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendDayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendsValidatorCreation()
        {
            _daysOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _daysOffPreferences.UseFullWeekendsOff = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_daysOffPreferences, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendValidator), _target.BuildActiveValidatorList()[0]);
        }

        #endregion
    }
}
