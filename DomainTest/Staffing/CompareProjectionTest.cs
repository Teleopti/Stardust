using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


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
		public MutableNow Now;
		public IScheduleStorage ScheduleStorage;

		[Test]
		public void ShouldThrowIfScheduleDaysAreNotForSamePerson()
		{
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var person = PersonRepository.Has(skill);
			var person2 = PersonRepository.Has(skill);

			var minResSkill = SkillRepository.Has("minResSkill", activity).WithId();
			minResSkill.DefaultResolution = 15;

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, assignmentPeriod, new ShiftCategory("category")));

			var scheduleDay1 = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));
			var scheduleDay2 = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person2, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person2].ScheduledDay(new DateOnly(now));
			
			Assert.That(() => Target.Compare(scheduleDay1, scheduleDay2), Throws.Exception);
		}

		[Test]
		public void ShouldThrowIfScheduleDaysAreNotForSameDate()
		{
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var person = PersonRepository.Has(skill);

			var minResSkill = SkillRepository.Has("minResSkill", activity).WithId();
			minResSkill.DefaultResolution = 15;

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 2, 8, 2017, 5, 2, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod2, new ShiftCategory("category")));

			var scheduleDay1 = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(assignmentPeriod.StartDateTime));
			var scheduleDay2 = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod2, scenario)[person].ScheduledDay(new DateOnly(assignmentPeriod2.StartDateTime));

			Assert.That(() => Target.Compare(scheduleDay1, scheduleDay2), Throws.Exception);
		}

		[Test]
		public void ShouldNotGiveAnythingIfSame()
		{
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);
			
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var person = PersonRepository.Has(skill);

			var minResSkill = SkillRepository.Has("minResSkill", activity).WithId();
			minResSkill.DefaultResolution = 15;

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category")));
			
			var scheduleRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person];
			var activityResourceIntervals = Target.Compare(scheduleRange.ScheduledDay(new DateOnly(now)), scheduleRange.ScheduledDay(new DateOnly(now)));
			activityResourceIntervals.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAddResourceWhenSameActivityButMoreResourcesAfter()
		{
		    const int resolution = 120;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
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
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod1.StartDateTime).Resource.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldRemoveResourceWhenSameActivityButLessResourcesAfter()
		{
			const int resolution = 120;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
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
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod1.StartDateTime).Resource.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldRemoveResourceWhenAddingNoSkillActivity()
		{
			const int resolution = 15;
			
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityNoSKill = ActivityRepository.Has("activityNoSkill");
			activityNoSKill.RequiresSkill = false;
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 30;

			var minResSkill = SkillRepository.Has("minResSkill", activity).WithId();
			minResSkill.DefaultResolution = resolution;

			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 11);
			var noSkillPeriod = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityNoSKill, noSkillPeriod);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));

			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == noSkillPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldAddResourceWhenRemovingNoSkillActivity()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityNoSKill = ActivityRepository.Has("activityNoSkill");
			activityNoSKill.RequiresSkill = false;
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
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
			activityResourceIntervals.First(x => x.StartDateTime == noSkillPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveResourceWhenAddingActivityThatDoesNotBelongToPersonSkill()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityOtherSkill = ActivityRepository.Has("activityOtherSkill");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var skill2 = SkillRepository.Has("skill2", activityOtherSkill).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityOtherSkill, assignmentPeriod);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));

			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldAddResourceWhenRemovingActivityThatDoesNotBelongToPersonSkill()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activityOtherSkill = ActivityRepository.Has("activityOtherSkill");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var skill2 = SkillRepository.Has("skill2", activityOtherSkill).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var afterCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityOtherSkill, assignmentPeriod);
			var before = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));

			var activityResourceIntervals = Target.Compare(before, afterCopy);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddAndRemoveResourceWhenDifferentActivities()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activityBefore = ActivityRepository.Has("activity");
			var activityAfter = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activityBefore).WithId();
			var skill2 = SkillRepository.Has("skill2", activityAfter).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill, skill2);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activityBefore, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay) ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityAfter, assignmentPeriod);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] {skill.Id.GetValueOrDefault()})).Resource.Should().Be.EqualTo(-1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] {skill2.Id.GetValueOrDefault()})).Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddResourceForEachActivityWhenChangedInInterval()
		{
			const int resolution = 120;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var skill2 = SkillRepository.Has("skill2", activity2).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill, skill2);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignmentPeriodTotal = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriodTotal, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activity2, assignmentPeriod2);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriodTotal.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(-0.5);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriodTotal.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill2.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldRemoveResourceForEachActivityWhenChangedInInterval()
		{
			const int resolution = 120;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var skill2 = SkillRepository.Has("skill2", activity2).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill, skill2);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignmentPeriodTotal = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriodTotal, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var afterCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activity2, assignmentPeriod2);
			var before = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(before, afterCopy);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriodTotal.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(0.5);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriodTotal.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill2.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldAddAndRemoveResourceWhenShiftIsMovedForward()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 11);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.MoveActivityAndKeepOriginalPriority(assignment.ShiftLayers.FirstOrDefault(), assignmentPeriod2.StartDateTime, new TrackedCommandInfo());
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod1.StartDateTime).Resource.Should().Be.EqualTo(-1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod1.EndDateTime).Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddAndRemoveResourceWhenShiftIsMovedBackward()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 11);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.MoveActivityAndKeepOriginalPriority(assignment.ShiftLayers.FirstOrDefault(), assignmentPeriod2.StartDateTime, new TrackedCommandInfo());
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(2);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod2.EndDateTime).Resource.Should().Be.EqualTo(-1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod2.StartDateTime).Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveResourceActivityIsMovedToOnNextDay()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 23, 2017, 5, 2, 1);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 2, 1, 2017, 5, 2, 2);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var afterCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activity, assignmentPeriod2);
			var before = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(before, afterCopy);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod2.StartDateTime).Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldGiveTheIntervalLengthOfTheSkill()
		{
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = 60;
			var minResSkill = SkillRepository.Has("minResSkill", activity).WithId();
			minResSkill.DefaultResolution = 15;
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
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod2.StartDateTime).Resource.Should().Be.EqualTo(1);
			activityResourceIntervals.First().StartDateTime.Should().Be.EqualTo(assignmentPeriod2.StartDateTime);
			activityResourceIntervals.First().EndDateTime.Should().Be.EqualTo(assignmentPeriod2.EndDateTime);
		}

		[Test]
		public void ShouldNotCreateDeltasForDeletedSkills()
		{
			const int resolution = 60;
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activityBefore = ActivityRepository.Has("activity");
			var activityAfter = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activityBefore).WithId();
			var skill2 = SkillRepository.Has("skill2", activityAfter).WithId();
			skill.DefaultResolution = skill2.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill, skill2);

			((IDeleteTag)skill2).SetDeleted();

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activityBefore, assignmentPeriod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(assignment);
			var beforeCopy = (IScheduleDay)ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now)).Clone();
			assignment.AddActivity(activityAfter, assignmentPeriod);
			var after = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person].ScheduledDay(new DateOnly(now));
			var activityResourceIntervals = Target.Compare(beforeCopy, after);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First(x => x.StartDateTime == assignmentPeriod.StartDateTime && x.SkillCombination.ToArray().SequenceEqual(new[] { skill.Id.GetValueOrDefault() })).Resource.Should().Be.EqualTo(-1);
		}
	}
}
