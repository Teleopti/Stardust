using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsCreatorTest1
    {
        private SchedulingOptionsCreator _target;
        private IOptimizationPreferences _optimizationPreferences;
        private ISchedulingOptions _schedulingOptions;

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
            _schedulingOptions.UseBlockScheduling = BlockFinderType.None;
            Assert.AreEqual(_schedulingOptions.UseBlockScheduling, BlockFinderType.None);
            _optimizationPreferences.Extra.UseBlockScheduling = true;
            _optimizationPreferences.Extra.BlockFinderTypeValue = BlockFinderType.BetweenDayOff;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.UseBlockScheduling, BlockFinderType.BetweenDayOff);
        }

        [Test]
        public void ShouldUseGroupingChangesSetInSchedulingOptions()
        {
            Assert.IsFalse(_schedulingOptions.UseGroupOptimizing);
            _optimizationPreferences.Extra.UseTeams = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseGroupOptimizing);
        }

        [Test]
        public void ShouldGroupOnGroupPageChangesSetInSchedulingOptions()
        {
            IGroupPage groupPage = new GroupPage("Test"); 
            Assert.AreNotEqual(_schedulingOptions.GroupOnGroupPage, groupPage);
            _optimizationPreferences.Extra.GroupPageOnTeam = groupPage;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.GroupOnGroupPage, groupPage);
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

        //[Test]
        //public void ShouldSetMushHavesTrue
    }
}
