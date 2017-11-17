using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageScheduleForecastSkillJobStep : JobStepBase
	{
		public StageScheduleForecastSkillJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_schedule_forecast_skill";
			JobCategory = JobCategoryType.Schedule;
			ScheduleForecastSkillInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			// Set start date one day earlier to be sure to get hold of all skill days
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor.AddDays(-1),
											JobCategoryDatePeriod.EndDateUtcCeiling);

			//Get data from Raptor
			IList<ISkill> skills =
				new List<ISkill>(
					_jobParameters.Helper.Repository.LoadSkillWithSkillDays(
						period.ToDateOnlyPeriod(TimeZoneInfo.Utc)));
			IList<IPerson> persons = _jobParameters.StateHolder.PersonCollection.ToList();
			var staffingCalculatorServiceFacade =
				_jobParameters.ContainerHolder.IocContainer.Resolve<IStaffingCalculatorServiceFacade>();
			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				var skillDaysDictionary =
					_jobParameters.StateHolder.GetSkillDaysDictionary(period, skills, scenario, staffingCalculatorServiceFacade);
				IScheduleDictionary scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				using (ISchedulingResultStateHolder schedulingResultStateHolder = new SchedulingResultStateHolder(persons,
																										   scheduleDictionary,
																										   skillDaysDictionary))
				{
					ISchedulingResultService schedulingResultService =
						new SchedulingResultService(schedulingResultStateHolder, skills, new CascadingPersonSkillProvider());
					IScheduleForecastSkillResourceCalculation scheduleForecastSkillResourceCalculation =
						new ScheduleForecastSkillResourceCalculation(new ShovelResources(new ReducePrimarySkillResources(), new AddResourcesToSubSkills(), new SkillSetPerActivityProvider(), new PrimarySkillOverstaff(), new TimeZoneGuard(), new AddBpoResourcesToContext()), 
																	skillDaysDictionary, schedulingResultService,
																	schedulingResultStateHolder.SkillStaffPeriodHolder,
																	schedulingResultStateHolder.Schedules,
																	skills,
																	 _jobParameters.IntervalsPerDay, period);

					//Transform data from Raptor to Matrix format
					var raptorTransformer = new ScheduleForecastSkillTransformer(_jobParameters.IntervalsPerDay,
																				 DateTime.Now);

					raptorTransformer.Transform(scheduleForecastSkillResourceCalculation, BulkInsertDataTable1);
				}
			}

			//Truncate stage table & Bulk insert data to stage database
			int retVal = _jobParameters.Helper.Repository.PersistScheduleForecastSkill(BulkInsertDataTable1);
			return retVal;
		}
	}
}
