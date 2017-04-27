using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
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
		public void ShouldRemoveBeforeResourcesOnInterval()
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
			var activityResourceIntervals = Target.Compare(scheduleRange.ScheduledDay(new DateOnly(now)), null);
			activityResourceIntervals.Count().Should().Be.EqualTo(4);
			activityResourceIntervals.First().Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldRemoveBeforeResourcesOnIntervalAndConsiderFractions()
		{
			IntervalLengthFetcher.Has(120);
			var now = new DateTime(2017, 5, 1, 0, 0, 0);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			var person = PersonRepository.Has(skill);

			var assignmentPeriod = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod, new ShiftCategory("category")));

			var scheduleRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod, scenario)[person];
			var activityResourceIntervals = Target.Compare(scheduleRange.ScheduledDay(new DateOnly(now)), null);
			activityResourceIntervals.Count().Should().Be.EqualTo(1);
			activityResourceIntervals.First().Resource.Should().Be.EqualTo(-0.5);
		}
	}
}
