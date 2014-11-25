using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class TeamBlockOpenHoursFilterTest
	{
		private TeamBlockOpenHoursFilter _target;
		private MockRepository _mock;
		private ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IShiftProjectionCache _shiftProjectionCache1;
		private IShiftProjectionCache _shiftProjectionCache2;
		private IList<IShiftProjectionCache> _shiftProjectionCaches;
		private ITeamBlockInfo _teamBlockInfo;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _matrixes;
		private IPerson _person;
		private List<IList<IScheduleMatrixPro>> _groupMatrixList;
		private Group _group;
		private ITeamInfo _teaminfo;
		private BlockInfo _blockInfo;
		private DateOnly _startDate;
		private DateOnly _endDate;
		private Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> _dateOnlyDictionary;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _activityDictionary1;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _activityDictionary2;
		private IList<ISkillIntervalData> _skillIntervalDatas1;
		private IList<ISkillIntervalData> _skillIntervalDatas2;
		private IActivity _activity;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			
			_createSkillIntervalDataPerDateAndActivity = _mock.StrictMock<ICreateSkillIntervalDataPerDateAndActivity>();
			_skillIntervalDataOpenHour = _mock.StrictMock<ISkillIntervalDataOpenHour>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_personalShiftMeetingTimeChecker = _mock.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();

			_activity = new Activity("activity1") {RequiresSkill = true};
			_startDate = new DateOnly(2014, 1, 1);
			_endDate = _startDate.AddDays(1);

			var shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1");
			var shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("shiftCategory2");

			var start1 = TimeSpan.FromHours(10);
			var end1 = TimeSpan.FromHours(14);
			var start2 = TimeSpan.FromHours(17);
			var end2 = TimeSpan.FromHours(19);
			var workShift1 = WorkShiftFactory.CreateWorkShift(start1, end1, _activity, shiftCategory1);
			var workShift2 = WorkShiftFactory.CreateWorkShift(start2, end2, _activity, shiftCategory2);

			_shiftProjectionCache1 = new ShiftProjectionCache(workShift1, _personalShiftMeetingTimeChecker);
			_shiftProjectionCache2 = new ShiftProjectionCache(workShift2, _personalShiftMeetingTimeChecker);
			_shiftProjectionCache1.SetDate(_startDate, TimeZoneInfo.Utc);
			_shiftProjectionCache2.SetDate(_endDate, TimeZoneInfo.Utc);
			_shiftProjectionCaches = new List<IShiftProjectionCache>{_shiftProjectionCache1, _shiftProjectionCache2};
	
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_person = PersonFactory.CreatePerson("Person");
			_groupMatrixList = new List<IList<IScheduleMatrixPro>> { _matrixes };
			_group = new Group(new List<IPerson> { _person }, "group");
			_teaminfo = new TeamInfo(_group, _groupMatrixList);
			_blockInfo = new BlockInfo(new DateOnlyPeriod(_startDate, _endDate));
			_teamBlockInfo = new TeamBlockInfo(_teaminfo, _blockInfo);
			_skillIntervalDatas1 = new List<ISkillIntervalData>();
			_skillIntervalDatas2 = new List<ISkillIntervalData>();
			_activityDictionary1 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			_activityDictionary1.Add(_activity, _skillIntervalDatas1);
			_activityDictionary2 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			_activityDictionary2.Add(_activity, _skillIntervalDatas2);
			_dateOnlyDictionary = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			_dateOnlyDictionary.Add(_startDate, _activityDictionary1);
			_dateOnlyDictionary.Add(_endDate, _activityDictionary2);

			_target = new TeamBlockOpenHoursFilter(_createSkillIntervalDataPerDateAndActivity, _skillIntervalDataOpenHour, _schedulingResultStateHolder);
		}

		[Test]
		public void ShouldFilterOutProjectionsNotInsideOpenHours()
		{
			TimePeriod? timePeriod1 = new TimePeriod(8, 0, 20, 0);
			TimePeriod? timePeriod2 = new TimePeriod(8, 0, 18, 0);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(timePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas2, _endDate)).Return(timePeriod2).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Filter(_shiftProjectionCaches, _teamBlockInfo, new WorkShiftFinderResultForTest());
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(_shiftProjectionCache1, result[0]);
			}
		}

		[Test]
		public void ShouldFilterOutProjectionsWhenNoOpenHours()
		{
			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(null).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Filter(_shiftProjectionCaches, _teamBlockInfo, new WorkShiftFinderResultForTest());
				Assert.AreEqual(0, result.Count);
			}	
		}
	}
}
