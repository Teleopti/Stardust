using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				IDictionary<ISkill, IList<ISkillDay>> skillDaysDictionary =
					_jobParameters.StateHolder.GetSkillDaysDictionary(period, skills, scenario);
				IScheduleDictionary scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				using (ISchedulingResultStateHolder schedulingResultStateHolder = new SchedulingResultStateHolder(persons,
																										   scheduleDictionary,
																										   skillDaysDictionary))
				{
					ISchedulingResultService schedulingResultService =
						new SchedulingResultService(schedulingResultStateHolder, skills, false, new PersonSkillProvider());
					DateTimePeriod visiblePeriod = scheduleDictionary.Period.VisiblePeriod;
					IScheduleForecastSkillResourceCalculation scheduleForecastSkillResourceCalculation =
						new ScheduleForecastSkillResourceCalculation(skillDaysDictionary, schedulingResultService,
																	 schedulingResultStateHolder.SkillStaffPeriodHolder.
																		 SkillStaffPeriodList(skills, visiblePeriod),
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
