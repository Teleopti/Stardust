using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
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
	[TestFixture]
	public class AddOverTimeHandlerTest
	{
		public AddOverTimeHandler Target;
		public MutableNow Now;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public IScheduleStorage ScheduleStorage;

		[Test]  // not working, should assert that overtime is added
		public void ShouldAddOverTimeForPeriodAndSkill()
		{
			Now.Is("2017-1-11 07:00");
			var period = new DateTimePeriod(2017, 1, 11, 8, 2017, 01, 11, 9);
			
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 5));

			Target.Handle(new AddOverTimeEvent {Period = period, Skills = new [] {skill.Id.GetValueOrDefault()}});
		var scheduleRange =	ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false), period.ToDateOnlyPeriod(agent.PermissionInformation.DefaultTimeZone()), scenario);
		}
	}
}
