using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class CompareProjectionTest
	{
		public CompareProjection Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;
		public IScheduleStorage ScheduleStorage;

		[Test]
		public void ShouldNotDoAnythingIfSame()
		{
			IntervalLengthFetcher.Has(15);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);
			
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category")));
			
			var scheduleRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person];
			var activityResourceIntervals = Target.Compare(scheduleRange.ScheduledDay(new DateOnly(now)), scheduleRange.ScheduledDay(new DateOnly(now)));
			activityResourceIntervals.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAddResourceWhenSameActivityButMoreResourcesAfter()
		{
			IntervalLengthFetcher.Has(120);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 60;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activity, assignmentPeriod2);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.Interval.StartDateTime == assignmentPeriod1.StartDateTime).Resource.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldRemoveResourceWhenSameActivityButLessResourcesAfter()
		{
			IntervalLengthFetcher.Has(120);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 60;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var afterCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activity, assignmentPeriod2);
			var before = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(before, afterCopy);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.Interval.StartDateTime == assignmentPeriod1.StartDateTime).Resource.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldRemoveResourceWhenAddingNoSkillActivity()
		{
			IntervalLengthFetcher.Has(60);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityNoSKill = ActivityRepository.Has("activityNoSkill");
			activityNoSKill.RequiresSkill = false;
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 60;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 11);
			var noSkillPeriod = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityNoSKill, noSkillPeriod);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));

			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.Interval.StartDateTime == noSkillPeriod.StartDateTime).Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldAddResourceWhenRemovingNoSkillActivity()
		{
			IntervalLengthFetcher.Has(60);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityNoSKill = ActivityRepository.Has("activityNoSkill");
			activityNoSKill.RequiresSkill = false;
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 60;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 11);
			var noSkillPeriod = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var afterCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityNoSKill, noSkillPeriod);
			var before = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));

			var activityResourceIntervals = Target.Compare(before, afterCopy);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.Interval.StartDateTime == noSkillPeriod.StartDateTime).Resource.Should().Be.EqualTo(1);
		}

		//activity change in the middle of interval
	}
}
