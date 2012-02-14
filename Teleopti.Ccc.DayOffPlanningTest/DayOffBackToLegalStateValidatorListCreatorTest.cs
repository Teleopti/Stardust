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

        #region Variables

        private DayOffBackToLegalStateValidatorListCreator _target;
        private IDayOffPlannerRules _dayOffPlannerRules;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodIndexRange;

        #endregion

        [SetUp]
        public void Setup()
        {
            _dayOffPlannerRules = new DayOffPlannerRules();
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));
            _periodIndexRange = new MinMax<int>(13, 19);
        }

        #region Build Active Validators tests

        [Test]
        public void VerifyConsecutiveDaysOffValidatorCreation()
        {
            _dayOffPlannerRules = DayOffPlannerRulesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPlannerRules.UseConsecutiveDaysOff = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof (ConsecutiveDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyConsecutiveWorkdaysValidatorCreation()
        {
            _dayOffPlannerRules = DayOffPlannerRulesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPlannerRules.UseConsecutiveWorkdays = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(ConsecutiveWorkdayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyDaysOffPerWeekValidatorCreation()
        {
            _dayOffPlannerRules = DayOffPlannerRulesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPlannerRules.UseDaysOffPerWeek = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(WeeklyDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendDayValidatorCreation()
        {
            _dayOffPlannerRules = DayOffPlannerRulesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPlannerRules.UseFreeWeekendDays = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendDayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendsValidatorCreation()
        {
            _dayOffPlannerRules = DayOffPlannerRulesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPlannerRules.UseFreeWeekends = true;
            _target = new DayOffBackToLegalStateValidatorListCreator(_dayOffPlannerRules, _officialWeekendDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendValidator), _target.BuildActiveValidatorList()[0]);
        }

        #endregion
    }
}
