using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateMainShiftSchedulerTest
	{
		private MockRepository _mocks;
		private IGroupMatrixHelper _groupMatrixHelper;
		private TeamSteadyStateMainShiftScheduler _target;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IGroupPerson _groupPerson;
		private IGroupSchedulingService _groupSchedulingService;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingOptions _schedulingOptions;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IList<IScheduleMatrixPro> _matrixes;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange1;
		private IScheduleRange _scheduleRange2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IPersonAssignment _personAssignment1;
		private IMainShift _mainShift1;
		private IMainShift _mainShift2;
		private IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private ScheduleTagSetter _scheduleTagSetter;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupMatrixHelper = _mocks.StrictMock<IGroupMatrixHelper>();
			_dateOnly = new DateOnly(2012, 1, 1);
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
			_groupPerson = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "groupPerson1");
			_groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_matrixes = new List<IScheduleMatrixPro>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			_mainShift1 = _mocks.StrictMock<IMainShift>();
			_scheduleRange2 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_mainShift2 = _mocks.StrictMock<IMainShift>();
			_scheduleDayChangeCallback = _mocks.StrictMock<IScheduleDayChangeCallback>();
			_scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);
			_target = new TeamSteadyStateMainShiftScheduler(_groupMatrixHelper, _scheduleDayChangeCallback, _scheduleTagSetter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldScheduleAllMembersWithSameMainShift()
		{
			using(_mocks.Record())
			{
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes)).Return(true);
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.AssignmentHighZOrder()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.MainShift).Return(_mainShift1);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person1, _dateOnly)).Return(_groupPerson);
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
				Expect.Call(_mainShift1.NoneEntityClone()).Return(_mainShift2);
				Expect.Call(()=>_scheduleDay2.AddMainShift(_mainShift2));
				Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.AutomaticScheduling, new List<IScheduleDay> { _scheduleDay2 }, null, _scheduleDayChangeCallback,_scheduleTagSetter)).Return(new List<IBusinessRuleResponse>());	
			}

			using(_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotScheduleMembersIfFirstPersonDidNotSchedule()
		{
			using(_mocks.Record())
			{
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes)).Return(false);	
			}

			using(_mocks.Playback())
			{
				var result = _target.ScheduleTeam(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, _scheduleDictionary);
				Assert.IsFalse(result);
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenGroupPersonBuilderIsNull()
		{
			_target.ScheduleTeam(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, null, _matrixes, _scheduleDictionary);		
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenScheduleDictionaryIsNull()
		{
			_target.ScheduleTeam(_dateOnly, _person1, _groupSchedulingService, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _matrixes, null);
		}
	}
}
