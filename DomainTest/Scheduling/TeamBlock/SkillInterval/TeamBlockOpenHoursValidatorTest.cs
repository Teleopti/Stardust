using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class TeamBlockOpenHoursValidatorTest
	{
		private TeamBlockOpenHoursValidator _target;
		private MockRepository _mock;
		private ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
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
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> _dateOnlyDictionary;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _activityDictionary1;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _activityDictionary2;
		private IList<ISkillIntervalData> _skillIntervalDatas1;
		private IList<ISkillIntervalData> _skillIntervalDatas2;
		private IActivity _activity;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_createSkillIntervalDataPerDateAndActivity = _mock.StrictMock<ICreateSkillIntervalDataPerDateAndActivity>();
			_skillIntervalDataOpenHour = _mock.StrictMock<ISkillIntervalDataOpenHour>();
			_target = new TeamBlockOpenHoursValidator(_createSkillIntervalDataPerDateAndActivity, _skillIntervalDataOpenHour);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();

			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_person = PersonFactory.CreatePerson("Person");
			_groupMatrixList = new List<IList<IScheduleMatrixPro>> { _matrixes };
			_group = new Group(new List<IPerson> { _person }, "group");
			_teaminfo = new TeamInfo(_group, _groupMatrixList);
			_startDate = new DateOnly(2014, 1, 1);
			_endDate = _startDate.AddDays(1);
			_blockInfo = new BlockInfo(new DateOnlyPeriod(_startDate, _endDate));
			_teamBlockInfo = new TeamBlockInfo(_teaminfo, _blockInfo);
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_activity = new Activity("activity");
			_skillIntervalDatas1 = new List<ISkillIntervalData>();
			_skillIntervalDatas2 = new List<ISkillIntervalData>();
			_activityDictionary1 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			_activityDictionary1.Add(_activity,_skillIntervalDatas1);
			_activityDictionary2 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			_activityDictionary2.Add(_activity, _skillIntervalDatas2);
			_dateOnlyDictionary = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			_dateOnlyDictionary.Add(_startDate,_activityDictionary1);
			_dateOnlyDictionary.Add(_endDate, _activityDictionary2);
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldReturnTrueWhenAllActivitiesHaveSameOpenHoursOnAllDates()
		{
			TimePeriod? timePeriod1 = new TimePeriod(8, 0, 17, 0);
			TimePeriod? timePeriod2 = new TimePeriod(8, 0, 17, 0);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate.AddDays(1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(timePeriod1);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas2, _endDate)).Return(timePeriod2);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenActivitiesDontHaveSameOpenHoursOnAllDates()
		{
			TimePeriod? timePeriod1 = new TimePeriod(8, 0, 17, 0);
			TimePeriod? timePeriod2 = new TimePeriod(12, 0, 16, 0);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate.AddDays(1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(timePeriod1);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas2, _endDate)).Return(timePeriod2);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenActivitiyClosedOnOneDay()
		{
			TimePeriod? timePeriod1 = new TimePeriod(8, 0, 17, 0);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate.AddDays(1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(timePeriod1);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas2, _endDate)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueWhenActivitiyClosedOnAllDays()
		{
			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate.AddDays(1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(null);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas2, _endDate)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotConsiderDaysOff()
		{
			TimePeriod? timePeriod1 = new TimePeriod(8, 0, 17, 0);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder)).Return(_dateOnlyDictionary);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_startDate.AddDays(1))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(true);
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDatas1, _startDate)).Return(timePeriod1);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}
	}
}
