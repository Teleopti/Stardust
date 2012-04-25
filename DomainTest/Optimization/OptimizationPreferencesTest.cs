using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{

    [TestFixture]
    public class OptimizationPreferencesTest
    {
        private OptimizationPreferences _target;

        [SetUp]
        public void Setup()
        {
            _target = new OptimizationPreferences();
        }

        [Test]
        public void ShouldAllSubSettingsBeInitializedInConstructor()
        {
            Assert.IsNotNull(_target.General);
            Assert.IsNotNull(_target.DaysOff);
            Assert.IsNotNull(_target.Extra);
            Assert.IsNotNull(_target.Advanced);
            Assert.IsNotNull(_target.Rescheduling);
        }

        [Test]
        public void VerifyGeneralSettingsDefaultValues()
        {
            Assert.IsTrue(_target.General.OptimizationStepDaysOff);
            Assert.IsTrue(_target.General.OptimizationStepShiftsWithinDay);
            Assert.IsTrue(_target.General.OptimizationStepTimeBetweenDays);
            Assert.IsTrue(_target.General.UsePreferences);
            Assert.IsTrue(_target.General.UseMustHaves);
            Assert.IsTrue(_target.General.UseRotations);
            Assert.IsTrue(_target.General.UseAvailabilities);
            Assert.IsTrue(_target.General.UseStudentAvailabilities);
            Assert.IsTrue(_target.General.UseShiftCategoryLimitations);

            Assert.IsFalse(_target.General.OptimizationStepShiftsForFlexibleWorkTime);
            Assert.IsFalse(_target.General.OptimizationStepDaysOffForFlexibleWorkTime);

        }

        [Test]
        public void VerifyDayOffSettingsDefaultValues()
        {
            Assert.IsTrue(_target.DaysOff.UseDaysOffPerWeek);
            Assert.IsTrue(_target.DaysOff.UseConsecutiveDaysOff);
            Assert.IsTrue(_target.DaysOff.UseConsecutiveWorkdays);
            Assert.IsTrue(_target.DaysOff.UseDaysOffPerWeek);
            //Assert.IsTrue(_target.DaysOff.ConsiderWeekBefore);
        }


        [Test]
        public void VerifyExtraSettingsDefaultValues()
        {
            Assert.AreEqual(_target.Extra.BlockFinderTypeValue, BlockFinderType.BetweenDayOff);
        }

        [Test]
        public void VerifyLocalSchedulingOptionsDefaultValues()
        {
            Assert.IsTrue(_target.Rescheduling.ConsiderShortBreaks);
        }
    }
}
