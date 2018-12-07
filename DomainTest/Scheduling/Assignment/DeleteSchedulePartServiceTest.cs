using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
    public class DeleteSchedulePartServiceTest
    {
        private MockRepository _mocks;
        private DeleteSchedulePartService _deleteService;
        private IList<IScheduleDay> _list;
        private IScheduleDay _part1;
        private IScheduleDay _part2;
        private IScheduleDay _part3;
        private DeleteOption _deleteOption;
    	private ISchedulePartModifyAndRollbackService _rollbackService;
        private NoSchedulingProgress _schedulingProgress;

		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public ITimeZoneGuard TimeZoneGuard;

		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _deleteService = new DeleteSchedulePartService();
        	_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_part1 = _mocks.StrictMock<IScheduleDay>();
			_part2 = _mocks.StrictMock<IScheduleDay>();
			_part3 = _mocks.StrictMock<IScheduleDay>();
            _list = new List<IScheduleDay>{_part1, _part2};
            _deleteOption = new DeleteOption();
            _schedulingProgress = new NoSchedulingProgress();
        }

		[Test]
        public void VerifyCanCreateObject()
        {
            Assert.IsNotNull(_deleteService);
        }

        [Test]
        public void VerifyDeleteMainShift()
        {
            using (_mocks.Record())
            {
                _part3.DeleteMainShift();
                _part3.DeleteMainShift();
                Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
                Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _deleteOption.MainShift = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

		[Test]
		public void VerifyDeleteMainShiftSpecial()
		{
			using (_mocks.Record())
			{
				_part3.DeleteMainShiftSpecial();
				_part3.DeleteMainShiftSpecial();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_deleteOption.MainShiftSpecial = true;
				_deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
			}
		}

        [Test]
        public void VerifyDeletePersonalStuff()
        {
            using (_mocks.Record())
            {
                _part3.DeletePersonalStuff();
                _part3.DeletePersonalStuff();


				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

            using (_mocks.Playback())
            {
                _deleteOption.PersonalShift = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteDayOff()
        {
            using (_mocks.Record())
            {
                _part3.DeleteDayOff();
                _part3.DeleteDayOff();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.DayOff = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void ShouldNotDeleteDayOffUnderFullDayAbsence()
        {
            using (_mocks.Record())
            {
            	Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();

                Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();


				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.Default = true;

                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteFullDayAbsence()
        {
            using (_mocks.Record())
            {
                _part3.DeleteFullDayAbsence(_part3);
                _part3.DeleteFullDayAbsence(_part3);

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.Absence = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteDefault()
        {
            using(_mocks.Record())
            {
                Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Absence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.PersonalShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.DayOff).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Overtime).Repeat.Twice();

                Expect.Call(() => _part3.DeleteMainShift()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeletePersonalStuff()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteDayOff()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteAbsence(false)).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteOvertime()).Repeat.AtLeastOnce();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

            using (_mocks.Playback())
            {
                _deleteOption.Default = true;

                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

		[Test]
		public void VerifyDeleteDefaultWithoutBackgroundWorker()
		{
			using (_mocks.Record())
			{
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Absence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.PersonalShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.DayOff).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Overtime).Repeat.Twice();

				Expect.Call(() => _part3.DeleteMainShift()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeletePersonalStuff()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteDayOff()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteAbsence(false)).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteOvertime()).Repeat.AtLeastOnce();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{

				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
			}
		}

        [Test]
        public void VerifyDeleteOvertime()
        {
            _list = new List<IScheduleDay> { _part1};
            using (_mocks.Record())
            {
                Expect.Call(_part1.ReFetch()).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.DeleteOvertime).Repeat.AtLeastOnce();

				Expect.Call(() => _rollbackService.Modify(_part1));
            }

            using (_mocks.Playback())
            {
                _deleteOption.Overtime = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }


		[Test]
		public void VerifyDeleteOvertimePoo()
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId());
			personAssignment.AddOvertimeActivity(activity, date.ToDateTimePeriod(new TimePeriod(8, 17), TimeZoneInfo.Utc), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Overtime = true}, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().OvertimeActivities().Should().Be.Empty();
		}

		[Test]
        public void VerifyDeletePreference()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var absenceRestriction = new PreferenceRestriction { Absence = new Absence().WithId() };
			var preferenceDay = new PreferenceDay(agent, date, absenceRestriction).WithId();

			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { preferenceDay }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Preference = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonRestrictionCollection().Should().Be.Empty();
		}

		[Test]
        public void VerifyStudentAvailabilityRestriction()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null), EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(20)) };
			var studentAvailabilityDay = new StudentAvailabilityDay(agent, date, new IStudentAvailabilityRestriction[] { studentAvailabilityRestriction });
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { studentAvailabilityDay }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { StudentAvailability = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonRestrictionCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteWithSpecifiedRules()
		{
			var rules = NewBusinessRuleCollection.Minimum();
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment1 = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAssignment2 = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment1, personAssignment2 }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			var result = target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress(), rules);

			result.Count.Should().Be.EqualTo(2);
		}

        [Test]
        public void VerifyDeleteIsReturningListOfNewScheduleParts()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment1 = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAssignment2 = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment1, personAssignment2 }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			var result = target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			result.Count.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotDeleteAbsenceFromDayBefore()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(20, 27), TimeZoneInfo.Utc)));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] {agent}, new []{personAbsence}, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
					new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(dateBefore.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldDeletePartDayAbsenceSpanningOverMidnight()
		{
			var target = new DeleteSchedulePartService();	
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(20, 27), TimeZoneInfo.Utc)));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAbsence }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}
	}
}
