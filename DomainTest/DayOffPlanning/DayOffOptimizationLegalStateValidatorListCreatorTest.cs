using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class DayOffOptimizationLegalStateValidatorListCreatorTest
    {
        private DayOffOptimizationLegalStateValidatorListCreator _target;
        private IDaysOffPreferences _dayOffPreferences;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodIndexRange;
        private BitArray _periodDays;

    	[SetUp]
        public void Setup()
        {
            _dayOffPreferences = new DaysOffPreferences();

            _officialWeekendDays = new OfficialWeekendDays();
            _periodIndexRange = new MinMax<int>(13, 19);
            _periodDays = new BitArray(28);
        }

    	[Test]
        public void VerifyConsecutiveDaysOffValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.UseConsecutiveDaysOff = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof (ConsecutiveDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyConsecutiveWorkdaysValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.UseConsecutiveWorkdays = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(ConsecutiveWorkdayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyDaysOffPerWeekValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.UseDaysOffPerWeek = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(WeeklyDayOffValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendDayValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.UseWeekEndDaysOff = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendDayValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyFreeWeekendsValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.UseFullWeekendsOff = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(FreeWeekendValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyKeepFreeWeekendsValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.KeepFreeWeekends = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(KeepFreeWeekendValidator), _target.BuildActiveValidatorList()[0]);
        }

        [Test]
        public void VerifyKeepFreeWeekendDaysValidatorCreation()
        {
            _dayOffPreferences = DaysOffPreferencesFactory.CreateWithFalseDefaultValues();
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(0, _target.BuildActiveValidatorList().Count);

            _dayOffPreferences.KeepFreeWeekendDays = true;
            _target = new DayOffOptimizationLegalStateValidatorListCreator(_dayOffPreferences, _officialWeekendDays, _periodDays, _periodIndexRange);
            _target.BuildActiveValidatorList();
            Assert.AreEqual(1, _target.BuildActiveValidatorList().Count);
            Assert.IsInstanceOf(typeof(KeepFreeWeekendDayValidator), _target.BuildActiveValidatorList()[0]);
        }
    }
}
