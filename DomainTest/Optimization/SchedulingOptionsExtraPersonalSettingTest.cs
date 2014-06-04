using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsExtraPersonalSettingTest
    {
        private SchedulingOptionsExtraPersonalSetting _target;
        private ISchedulingOptions _schedulingOptions;
        private IList<IScheduleTag> _scheduleTags;
        private IList<IGroupPageLight> _groupPages;
        private IScheduleTag _scheduleTag;
        private IGroupPageLight _groupPageLight;
        private Guid _guid;
        private List<IActivity> _activityList;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _schedulingOptions = new SchedulingOptions();
	        _scheduleTag = new ScheduleTag();
			_scheduleTags = new List<IScheduleTag>{_scheduleTag};
            _groupPageLight = new GroupPageLight();
	        _groupPageLight.Key = "hej";
            _groupPages = new List<IGroupPageLight> { _groupPageLight };
            _target = new SchedulingOptionsExtraPersonalSetting();
            _guid = Guid.NewGuid();
			_scheduleTag.SetId(_guid);
            _activity = new Activity("a1");
			_activity.SetId(_guid);
            _activityList = new List<IActivity>();
            _activityList.Add(_activity);
        }

	    [Test]
	    public void ValidateDefaultsWhenSettingsIsMissingInDatabase()
	    {
		    _target.MapTo(_schedulingOptions, _scheduleTags, _groupPages, _groupPages, _activityList);

			//Block
			Assert.AreEqual(BlockFinderType.SingleDay, _schedulingOptions.BlockFinderTypeForAdvanceScheduling);
			Assert.IsTrue(_schedulingOptions.BlockSameShiftCategory);
			Assert.IsFalse(_schedulingOptions.BlockSameStartTime);
			Assert.IsFalse(_schedulingOptions.BlockSameEndTime);
			Assert.IsFalse(_schedulingOptions.BlockSameShift);

			//Team
			Assert.IsNull(_schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			Assert.IsTrue(_schedulingOptions.TeamSameShiftCategory);
			Assert.IsFalse(_schedulingOptions.TeamSameStartTime);
			Assert.IsFalse(_schedulingOptions.TeamSameEndTime);
			Assert.IsFalse(_schedulingOptions.TeamSameStartTime);
			Assert.IsFalse(_schedulingOptions.TeamSameActivity);
			Assert.IsNull(_schedulingOptions.CommonActivity);

			//Fairness
			Assert.IsNull(_schedulingOptions.GroupPageForShiftCategoryFairness);
			Assert.AreEqual(new Percent(0), _schedulingOptions.Fairness);
	    }

        [Test]
        public void ShouldMap()
        {	_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
	        _schedulingOptions.BlockSameShiftCategory = true;
	        _schedulingOptions.BlockSameStartTime = true;
	        _schedulingOptions.BlockSameEndTime = true;
	        _schedulingOptions.BlockSameShift = true;

	        _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
	        _schedulingOptions.TeamSameShiftCategory = true;
	        _schedulingOptions.TeamSameStartTime = true;
	        _schedulingOptions.TeamSameEndTime = true;
	        _schedulingOptions.TeamSameActivity = true;
	        _schedulingOptions.CommonActivity = _activity;

	        _schedulingOptions.GroupPageForShiftCategoryFairness = _groupPageLight;
			_schedulingOptions.Fairness = new Percent(1);

            _target.MapFrom(_schedulingOptions);
			_schedulingOptions = new SchedulingOptions();
			_target.MapTo(_schedulingOptions, _scheduleTags, _groupPages, _groupPages, _activityList);

			//Block
			Assert.AreEqual(BlockFinderType.BetweenDayOff, _schedulingOptions.BlockFinderTypeForAdvanceScheduling);
			Assert.IsTrue(_schedulingOptions.BlockSameShiftCategory);
			Assert.IsTrue(_schedulingOptions.BlockSameStartTime);
			Assert.IsTrue(_schedulingOptions.BlockSameEndTime);
			Assert.IsTrue(_schedulingOptions.BlockSameShift);

			//Team
			Assert.AreEqual(_groupPageLight, _schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			Assert.IsTrue(_schedulingOptions.TeamSameShiftCategory);
			Assert.IsTrue(_schedulingOptions.TeamSameStartTime);
			Assert.IsTrue(_schedulingOptions.TeamSameEndTime);
			Assert.IsTrue(_schedulingOptions.TeamSameStartTime);
			Assert.IsTrue(_schedulingOptions.TeamSameActivity);
			Assert.AreEqual(_activity, _schedulingOptions.CommonActivity);

			//Fairness
			Assert.AreEqual(_groupPageLight, _schedulingOptions.GroupPageForShiftCategoryFairness);
			Assert.AreEqual(new Percent(1), _schedulingOptions.Fairness);

        }

        [Test]
        public void ShouldSetTagToNullScheduleInstanceWhenNoTag()
        {
	        _schedulingOptions.TagToUseOnScheduling = null;
			_target.MapTo(_schedulingOptions, _scheduleTags, _groupPages, _groupPages, _activityList);
			Assert.AreEqual(NullScheduleTag.Instance, _schedulingOptions.TagToUseOnScheduling);
        }
    }
}
