using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningPeriodConfigurable : IDataSetup
	{
		public PlanningPeriod Period;
		public DateTime Date { get; set; }
		public string PlanningGroupName { get; set; }
		public SchedulePeriodType? Type { get; set; }
		public bool HasScheduled { get; set; }
		
		public void Apply (ICurrentUnitOfWork currentUnitOfWork)
		{

			var period = new AggregatedSchedulePeriod
			{
				DateFrom = Date,
				Culture = CultureInfo.CurrentCulture.LCID,
				PeriodType = Type ?? SchedulePeriodType.Month,
				Number = 1,
				Priority = 1
			};
			PlanningGroup planningGroup = null;
			if (PlanningGroupName != null)
				planningGroup = new PlanningGroupRepository(currentUnitOfWork).LoadAll().FirstOrDefault(a => a.Name == PlanningGroupName);
			
			var planningPeriodSuggestions = new PlanningPeriodSuggestions (new MutableNow (Date),new[] {period});
			
			Period = new PlanningPeriod (planningPeriodSuggestions, planningGroup);

			if (HasScheduled)
			{
				var personRepository = new PersonRepository(currentUnitOfWork, null, null);
				var jobResult = new JobResult(JobCategory.WebSchedule, Period.Range, personRepository.LoadAll().FirstOrDefault(), DateTime.UtcNow)
				{
					FinishedOk = true
				};
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, 
					"{\"ScheduledAgentsCount\":1,\"BusinessRulesValidationResults\":[],\"SkillResultList\":[{\"SkillName\":\"Skill 1\",\"SkillDetails\":[{\"Date\":{\"Date\":\"2016-06-08T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-09T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-10T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-11T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-12T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-13T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2},{\"Date\":{\"Date\":\"2016-06-14T00:00:00\"},\"RelativeDifference\":-0.82222222222222219,\"ColorId\":2}]}]}",
					DateTime.UtcNow, null));
				new JobResultRepository(currentUnitOfWork).Add(jobResult);
				Period.JobResults.Add(jobResult);
				var scenario = DefaultScenario.Scenario;
				var activity = ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().FirstOrDefault();
				var perons = personRepository.FindPeopleInPlanningGroup(planningGroup, Period.Range);
				var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(currentUnitOfWork);
				foreach (var person in perons)
				{
					foreach (var date in Period.Range.DayCollection())
					{
						var startTimeUtc = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date.Date.AddHours(8));
						var endTimeUtc = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date.Date.AddHours(16));
						var assignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(person, scenario, activity, new DateTimePeriod(startTimeUtc, endTimeUtc));
						personAssignmentRepository.Add(assignment);
					}
				}
			}

			new PlanningPeriodRepository(currentUnitOfWork).Add(Period);
		}
	}
}