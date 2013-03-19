﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamDayOffModifyerTest
	{
		private MockRepository _mocks;
		private ITeamDayOffModifyer _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private ISchedulingResultStateHolder _stateHolder;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ITeamInfo _teamInfo;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IScheduleDictionary _dic;
		private IPerson _person1;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_target = new TeamDayOffModifyer(_resourceOptimizationHelper, _stateHolder);
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), DateOnly.MinValue);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), DateOnly.MinValue);
			IList<IPerson> members = new List<IPerson>{_person1, _person2};
			IGroupPerson groupPerson = new GroupPerson(members, DateOnly.MinValue, "hej", null);
			_teamInfo = new TeamInfo(groupPerson, new List<IList<IScheduleMatrixPro>>());
			_schedulingOptions = new SchedulingOptions {UseSameDayOffs = true};
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			IDictionary<IPerson, IScheduleRange> ranges = new Dictionary<IPerson, IScheduleRange>();
			ranges.Add(_person1, _range1);
			ranges.Add(_person2, _range2);
			_dic = new ScheduleDictionaryForTest(new Scenario("s"),
																	new ScheduleDateTimePeriod(new DateTimePeriod()), ranges);
		}

		[Test]
		public void ShouldAddOnEveryTeamMemberAndResourceCalculate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(_dic);

				Expect.Call(_range1.ScheduledDay(DateOnly.MinValue)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.Clone()).Return(_scheduleDay1);
				Expect.Call(() => _scheduleDay1.DeleteMainShift(_scheduleDay1));
				Expect.Call(() => _scheduleDay1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay1));
				Expect.Call(_range1.ReFetch(_scheduleDay1)).Return(_scheduleDay1);

				Expect.Call(_range2.ScheduledDay(DateOnly.MinValue)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.Clone()).Return(_scheduleDay2);
				Expect.Call(() => _scheduleDay2.DeleteMainShift(_scheduleDay2));
				Expect.Call(() => _scheduleDay2.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _rollbackService.Modify(_scheduleDay2));
				Expect.Call(_range2.ReFetch(_scheduleDay2)).Return(_scheduleDay2);

				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true,
				                                                              _schedulingOptions.ConsiderShortBreaks,
				                                                              new List<IScheduleDay> {_scheduleDay1, _scheduleDay2},
				                                                              new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}));
			}

			using (_mocks.Playback())
			{
				_target.AddDayOffAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue, _schedulingOptions);
			}
		}

		[Test]
		public void ShouldRemoveOnEveryTeamMemberAndResourceCalculate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(_dic);

				Expect.Call(_range1.ScheduledDay(DateOnly.MinValue)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.Clone()).Return(_scheduleDay1);
				Expect.Call(() => _scheduleDay1.DeleteDayOff());
				Expect.Call(() => _rollbackService.Modify(_scheduleDay1));
				Expect.Call(_range1.ReFetch(_scheduleDay1)).Return(_scheduleDay1);

				Expect.Call(_range2.ScheduledDay(DateOnly.MinValue)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.Clone()).Return(_scheduleDay2);
				Expect.Call(() => _scheduleDay2.DeleteDayOff());
				Expect.Call(() => _rollbackService.Modify(_scheduleDay2));
				Expect.Call(_range2.ReFetch(_scheduleDay2)).Return(_scheduleDay2);

				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true,
																			  _schedulingOptions.ConsiderShortBreaks,
																			  new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 },
																			  new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 }));
			}

			using (_mocks.Playback())
			{
				_target.RemoveDayOffAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue, _schedulingOptions);
			}
		}

	}
}