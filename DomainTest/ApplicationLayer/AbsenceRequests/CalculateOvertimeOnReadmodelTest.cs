using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
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

		[Test, Ignore("WIP")]
		public void ShouldCalculateNewResourceOnOvertimeInterval()
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
			newStaffing.Count.Should().Be.EqualTo(4);
			newStaffing[0].NewStaffing.Should().Be(11);
			newStaffing[1].NewStaffing.Should().Be(11);
		}

		[Test, Ignore("WIP")]
		public void ShouldCalculateNewResourceUntilThereIsDemand()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skillPrivate = SkillRepository.Has("private", activity).WithId();
			skillPrivate.DefaultResolution = 30;
			var agent1 = PersonRepository.Has(skillPrivate);
			var agent2 = PersonRepository.Has(skillPrivate);
			var agent3 = PersonRepository.Has(skillPrivate);
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);
			PersonOnSkillProvider.FakePersons = new List<IPerson>() { agent1,agent2,agent3 };
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent1, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent2, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent3, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime,period.StartDateTime.AddMinutes(30)), new[] { skillPrivate.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30),period.StartDateTime.AddMinutes(60)), new[] { skillPrivate.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(60),period.StartDateTime.AddMinutes(90)), new[] { skillPrivate.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(90),period.StartDateTime.AddMinutes(120)), new[] { skillPrivate.Id.GetValueOrDefault()}, 10)
			});



			ScheduleForecastSkillReadModelRepository.Persist(new[]
			{
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.StartDateTime.AddMinutes(30),
					Forecast = 12,
					SkillId = skillPrivate.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(30),
					EndDateTime = period.StartDateTime.AddMinutes(60),
					Forecast = 12,
					SkillId = skillPrivate.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(60),
					EndDateTime = period.StartDateTime.AddMinutes(90),
					Forecast = 12,
					SkillId = skillPrivate.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					StartDateTime = period.StartDateTime.AddMinutes(90),
					EndDateTime = period.StartDateTime.AddMinutes(120),
					Forecast = 12,
					SkillId = skillPrivate.Id.GetValueOrDefault()
				}
			}, DateTime.Now);

			var newStaffing = Target.CalculateOvertimeResource(new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 11), new[] { skillPrivate.Id.GetValueOrDefault() });
			newStaffing.Count.Should().Be.EqualTo(4);
			newStaffing[0].NewStaffing.Should().Be(12);
			newStaffing[1].NewStaffing.Should().Be(12);
			newStaffing[2].NewStaffing.Should().Be(2);
			newStaffing[3].NewStaffing.Should().Be(2);
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
			skillStaffingIntervals.ForEach(x =>
			{
				x.StaffingLevel = 0;
			});
			var allSkills = _skillRepository.LoadAll();
			var activtiesInvolved = allSkills.Where(x => skillIds.ToList().Contains((Guid)x.Id)).Select(a => a.Activity).Distinct();
			var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => allSkills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
							v =>
								(IResourceCalculationPeriodDictionary)
								new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
									s => (IResourceCalculationPeriod)s)));
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
					
					var skillsForActivity = allSkills.Where(x => x.Activity == activity).ToList();
					var skillInterval = skillsForActivity.Min(x => x.DefaultResolution);
					var resourcesForCalculation = new ResourcesExtractorCalculation(skillCombinations, allSkills, skillInterval);
					//var resourcesForShovel = new ResourcesExtractorShovel(combinationResources, allSkills, skillInterval);
					var scheduleResourceOptimizer = new ScheduleResourceOptimizer(resourcesForCalculation, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods), new AffectedPersonSkillService(allSkills), false, new ActivityDivider());
					
					var intervals = getSplitedIntervals(overtimePeriod,skillInterval);
					foreach (var interval in intervals)
					{
						scheduleResourceOptimizer.Optimize(interval);
						foreach (var skillForActivity in skillsForActivity)
						{
							//if (needForOvertime(interval, skillStaffingIntervals.Where(x => skillsForActivity.Select(s => s.Id.GetValueOrDefault()).Contains(x.SkillId))))
							var allIntervalsOnSkill = skillStaffingIntervals.Where(x => x.SkillId == skillForActivity.Id.GetValueOrDefault() && (x.StartDateTime==interval.StartDateTime && x.EndDateTime==interval.EndDateTime));
							if (allIntervalsOnSkill.Any())
							{
								allIntervalsOnSkill.ForEach(skillIntervalOnSkill =>
								{
									if (needForOvertime(interval, allIntervalsOnSkill))
									{
										var tempInterval = new SkillIntervalsForOvertime()
										{
											SkillId = skillForActivity.Id.GetValueOrDefault(),
											Time = interval.StartDateTime,
											NewStaffing = skillIntervalOnSkill.StaffingLevel +1
										};
										if (skillIntervalsForOvertime.Contains(tempInterval))
										{
											skillIntervalsForOvertime.First(x => x == tempInterval).NewStaffing++;
										}
										else
										{
											//var staffing = skillIntervalOnSkill.StaffingLevel;
											//if (needForOvertime(interval, allIntervalsOnSkill))
											skillIntervalsForOvertime.Add(tempInterval);
										}
									}
									
								});

								//NEED TO WRITE A TEST FOR THIS
								//var allAffectedCombinations = skillCombinations.Where(
								//	x => x.SkillCombination.NonSequenceEquals(personSkillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
								//		 && (interval.StartDateTime >= x.StartDateTime || interval.EndDateTime >= x.EndDateTime));
								//if (allAffectedCombinations.Any())
								//{
								//	allAffectedCombinations.ForEach(skillComb =>
								//	{
								//		skillComb.Resource++;
								//	});
								//}
								//else
								//{
								//	//create the new skillcom
								//}

							}
							else
							{
								skillIntervalsForOvertime.Add(new SkillIntervalsForOvertime()
								{
									SkillId = skillForActivity.Id.GetValueOrDefault(),
									Time = interval.StartDateTime,
									NewStaffing = 1
								});
							}
						}
						
					}
				}
			}


			return skillIntervalsForOvertime;
		}

		private bool needForOvertime(DateTimePeriod interval, IEnumerable<SkillStaffingInterval> skillStaffingIntervals)
		{
			//please check that
			var intervalsOnPeriod = skillStaffingIntervals.Where(x => x.StartDateTime >= interval.StartDateTime && x.EndDateTime <= interval.EndDateTime);
			return intervalsOnPeriod.Any(x => x.Forecast > x.StaffingLevel);
		}

		private IList<DateTimePeriod> getSplitedIntervals(DateTimePeriod period, int skillInterval)
		{
			var ret = new List<DateTimePeriod>() { new DateTimePeriod(period.StartDateTime.Utc(), period.StartDateTime.Add(TimeSpan.FromMinutes(skillInterval)).Utc()) };

			while (ret.Last().EndDateTime < period.EndDateTime)
			{
				var startDateTime = ret.Last().EndDateTime;
				var endDateTime = startDateTime.Add(TimeSpan.FromMinutes(skillInterval));
				ret.Add(new DateTimePeriod(startDateTime.Utc(), endDateTime.Utc()));
			}
			return ret;
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


	public class SkillIntervalsForOvertime  : IEquatable<SkillIntervalsForOvertime> ,IComparable<SkillIntervalsForOvertime>
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
		public bool Equals(SkillIntervalsForOvertime other)
		{
			return (SkillId == other.SkillId && Time == other.Time && NewStaffing == other.NewStaffing);
		}

		public int CompareTo(SkillIntervalsForOvertime other)
		{
			if( SkillId == other.SkillId && Time == other.Time && NewStaffing == other.NewStaffing)
				return 1;
			return 0;

		}
	}
}