using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class CalculateOvertimeOnReadmodelTest : ISetup
	{
		public CalculateOvertimeOnReadmodel Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakePersonOnSkillProvider PersonOnSkillProvider;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<CalculateOvertimeOnReadmodel>().For<ICalculateOvertimeOnReadmodel>();
			system.UseTestDouble<FakePersonOnSkillProvider>().For<IPersonOnSkillProvider>();
		}

		[Test, Ignore("Under Dev")]
		public void ShouldCalculateResourceOnInterval()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 30;
			var agent = PersonRepository.Has(skill);
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);
			PersonOnSkillProvider.FakePersons = new List<IPerson>() {agent};
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime,period.StartDateTime.AddMinutes(30)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30),period.StartDateTime.AddMinutes(60)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(60),period.StartDateTime.AddMinutes(90)), new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(90),period.StartDateTime.AddMinutes(120)), new[] {skill.Id.GetValueOrDefault()}, 10)
			});



			ScheduleForecastSkillReadModelRepository.Persist(new[]
			{
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.StartDateTime.AddMinutes(30),
					Forecast = 12,
					SkillId = skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(30),
					EndDateTime = period.StartDateTime.AddMinutes(60),
					Forecast = 12,
					SkillId = skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(60),
					EndDateTime = period.StartDateTime.AddMinutes(90),
					Forecast = 12,
					SkillId = skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(90),
					EndDateTime = period.StartDateTime.AddMinutes(120),
					Forecast = 12,
					SkillId = skill.Id.GetValueOrDefault()
				}
			}, DateTime.Now);

			var newStaffing =  Target.CalculateOvertimeResource(new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 11), new[] { skill.Id.GetValueOrDefault() });
			newStaffing.Count.Should().Be.Should().Be.EqualTo(4);
			newStaffing[0].NewStaffing.Should().Be(10);
			newStaffing[1].NewStaffing.Should().Be(10);
			newStaffing[2].NewStaffing.Should().Be(11);
			newStaffing[3].NewStaffing.Should().Be(11);
			newStaffing[4].NewStaffing.Should().Be(1);
			newStaffing[5].NewStaffing.Should().Be(1);
		}

		private static void createWfcs(WorkflowControlSet wfcs, IAbsence absence)
		{
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, Guid[] skillCombinations, double resource)
		{
			return new SkillCombinationResource
			{
				StartDateTime = period1.StartDateTime,
				EndDateTime = period1.EndDateTime,
				Resource = resource,
				SkillCombination = skillCombinations
			};
		}

	}

	public interface ICalculateOvertimeOnReadmodel
	{
		IList<SkillIntervalsForOvertime> CalculateOvertimeResource(DateTimePeriod overtimePeriod, Guid[] skillIds);
	}

	public class CalculateOvertimeOnReadmodel : ICalculateOvertimeOnReadmodel
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonOnSkillProvider _personOnSkillProvider;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IPersonSkillProvider _personSkillProvider;

		public CalculateOvertimeOnReadmodel(IScheduleStorage scheduleStorage, IPersonOnSkillProvider personOnSkillProvider, ICurrentScenario currentScenario,
			ISkillCombinationResourceRepository skillCombinationResourceRepository, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			ISkillRepository skillRepository, IPersonSkillProvider personSkillProvider)
		{
			_scheduleStorage = scheduleStorage;
			_personOnSkillProvider = personOnSkillProvider;
			_currentScenario = currentScenario;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_skillRepository = skillRepository;
			_personSkillProvider = personSkillProvider;
		}

		public IList<SkillIntervalsForOvertime> CalculateOvertimeResource(DateTimePeriod overtimePeriod, Guid[] skillIds)
		{
			var skillIntervalsForOvertime = new List<SkillIntervalsForOvertime>();
			var peopleOnSkill = _personOnSkillProvider.PeopleOnSkill(skillIds);
			//UTC please CHECK
			var dateOnlyPeriod = overtimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc);

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(peopleOnSkill,
				new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod,
				_currentScenario.Current());

			var skillCombinations = _skillCombinationResourceRepository.LoadSkillCombinationResources(overtimePeriod);
			var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(skillIds, overtimePeriod.StartDateTime, overtimePeriod.EndDateTime);
			var allSkills = _skillRepository.LoadAll();
			var activtiesInvolved = allSkills.Where(x => skillIds.ToList().Contains((Guid)x.Id)).Select(a => a.Activity).Distinct();
			foreach (var person in peopleOnSkill)
			{
				var personSchedule = schedules[person];
				var scheduleEndTime =
					personSchedule.ScheduledDay(dateOnlyPeriod.StartDate)
						.ProjectionService()
						.CreateProjection()
						.Last()
						.Period.EndDateTime;
				if (scheduleEndTime < overtimePeriod.StartDateTime || scheduleEndTime >= overtimePeriod.EndDateTime) continue;
				foreach (var activity in activtiesInvolved)
				{
					var personSkillCombination =
							_personSkillProvider.SkillsOnPersonDate(person, dateOnlyPeriod.StartDate)
								.ForActivity(activity.Id.GetValueOrDefault());
					if (!personSkillCombination.Skills.Any()) continue;
					var allAffectedCombinations = skillCombinations.Where(
						x => x.SkillCombination.NonSequenceEquals(personSkillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
							 && (overtimePeriod.StartDateTime >= x.StartDateTime || overtimePeriod.EndDateTime >= x.EndDateTime));
				}
			}


			return skillIntervalsForOvertime;
		}
	}

	public interface IPersonOnSkillProvider
	{
		IList<IPerson> PeopleOnSkill(Guid[] skillIds);
	}

	public class FakePersonOnSkillProvider : IPersonOnSkillProvider
	{
		public IList<IPerson> FakePersons { get; set; }
		public IList<IPerson> PeopleOnSkill(Guid[] skillIds)
		{
			return FakePersons;
		}
	}


	public class SkillIntervalsForOvertime
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
	}
}