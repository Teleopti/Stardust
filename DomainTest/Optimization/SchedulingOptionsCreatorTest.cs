using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsCreatorTest
    {
        private SchedulingOptionsCreator _target;
        private IOptimizationPreferences _optimizationPreferences;
        private SchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _target = new SchedulingOptionsCreator();
            _optimizationPreferences = new OptimizationPreferences();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void ShouldTagChangesSetInSchedulingOptions()
        {
            IScheduleTag tag = new ScheduleTag(); 
            Assert.AreNotEqual(_schedulingOptions.TagToUseOnScheduling, tag);
            _optimizationPreferences.General.ScheduleTag = tag;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.TagToUseOnScheduling, tag);
        }

        [Test]
        public void ShouldUseBlockSchedulingSetInSchedulingOptions()
        {
			  _optimizationPreferences.Extra.UseTeamBlockOption  = true;
            _optimizationPreferences.Extra.BlockTypeValue = BlockFinderType.BetweenDayOff;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.BlockFinderTypeForAdvanceScheduling  , BlockFinderType.BetweenDayOff);
        }

        [Test]
        public void ShouldUseGroupingChangesSetInSchedulingOptions()
        {
            Assert.IsFalse(_schedulingOptions.UseTeam);
            _optimizationPreferences.Extra.UseTeams = true;
	        _optimizationPreferences.Extra.UseTeamSameDaysOff = false;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
			Assert.IsTrue(_schedulingOptions.UseTeam);
			Assert.IsFalse(_schedulingOptions.UseSameDayOffs);
        }

        [Test]
        public void ShouldGroupOnGroupPageChangesSetInSchedulingOptions()
        {
            GroupPageLight groupPage = new GroupPageLight(); 
            _optimizationPreferences.Extra.TeamGroupPage = groupPage;
	        _optimizationPreferences.Extra.UseTeams = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
				Assert.AreEqual(_schedulingOptions.GroupOnGroupPageForTeamBlockPer, groupPage);
        }

        [Test]
        public void ShouldConsiderShortBreaksChangesSetInSchedulingOptions()
        {
            Assert.IsTrue(_schedulingOptions.ConsiderShortBreaks);
            _optimizationPreferences.Rescheduling.ConsiderShortBreaks = false;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.ConsiderShortBreaks);
        }

        [Test]
        public void ShouldOnlyShiftsWhenUnderstaffedChangesSetInSchedulingOptions()
        {
            Assert.IsFalse(_schedulingOptions.OnlyShiftsWhenUnderstaffed);
            _optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.OnlyShiftsWhenUnderstaffed);
        }

        [Test]
        public void VerifyMustHaves()
        {
            _optimizationPreferences.General.UsePreferences = false;

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);

            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
			Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0.8;
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
        }

        [Test]
        public void VerifyRotations()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsTrue(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);
        }

        [Test]
        public void VerifyAvailabilities()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsTrue(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 1;
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);
        }

        [Test]
        public void VerifyStudentAvailabilities()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);
        }

		[Test]
        public void VerifyTeamBlockOptions()
		{
			_optimizationPreferences.Extra.UseTeamSameDaysOff = false;

            _optimizationPreferences.Extra.UseTeamBlockOption = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
				Assert.IsTrue(_schedulingOptions.UseBlock);
			Assert.IsTrue(_schedulingOptions.UseSameDayOffs);

            _optimizationPreferences.Extra.UseBlockSameEndTime  = false;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.BlockSameEndTime);

            _optimizationPreferences.Extra.UseBlockSameShift  = false ;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
				Assert.IsFalse(_schedulingOptions.BlockSameShift);

            _optimizationPreferences.Extra.UseBlockSameShiftCategory  = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.BlockSameShiftCategory);

            _optimizationPreferences.Extra.UseBlockSameStartTime  = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.BlockSameStartTime);

        }

		[Test]
		public void ShouldAssignCorrectValueForTeamSchedulingWhenBlockIsUnchecked()
		{
			_optimizationPreferences.Extra.UseTeamBlockOption = false;
			_schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
			Assert.That(_schedulingOptions.BlockFinderTypeForAdvanceScheduling, Is.EqualTo(BlockFinderType.SingleDay));
		}
		
		[Test]
		public void ShouldAssignCorrectValueForBlockSchedulingWhenTeamIsUnchecked()
		{
			_optimizationPreferences.Extra.UseTeams = false;
			_schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
			Assert.That(_schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key, Is.EqualTo("SingleAgent"));
		}

	    [Test]
	    public void ShouldMapShiftBagBackToLegal()
	    {
		    _optimizationPreferences.ShiftBagBackToLegal = true;
			_schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
			_schedulingOptions.ShiftBagBackToLegal.Should().Be.True();

		}

    }
}
