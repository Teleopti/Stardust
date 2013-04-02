using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class StageScheduleDayOffCountJobStep : JobStepBase
	{
		private readonly INeedToRunChecker _needToRunChecker;

		public StageScheduleDayOffCountJobStep(IJobParameters jobParameters)
			: this(jobParameters, new DefaultNeedToRunChecker())
		{}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public StageScheduleDayOffCountJobStep(IJobParameters jobParameters, INeedToRunChecker needToRunChecker)
			: base(jobParameters)
		{
			_needToRunChecker = needToRunChecker;
			Name = "stg_schedule_day_off_count, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new ScheduleDayOffCountTransformer();
			DayOffSubStep = new EtlDayOffSubStep();
			ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IScheduleDayOffCountTransformer Transformer { get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor,
													   JobCategoryDatePeriod.EndDateUtcCeiling);

			if (!_needToRunChecker.NeedToRun(period, _jobParameters.Helper.Repository, Result.CurrentBusinessUnit, Name))
			{
				//gets resetted to Done later :(
				Result.Status = "No need to run";
				return 0;
			}

			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				//Get data from Raptor
				var scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				// Extract one schedulepart per each person and date
				var rootList = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(scheduleDictionary);

				//Transform data from Raptor to Matrix format
				Transformer.Transform(rootList, BulkInsertDataTable1, _jobParameters.IntervalsPerDay);
			}

			var rowsAffected = _jobParameters.Helper.Repository.PersistScheduleDayOffCount(BulkInsertDataTable1);
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.ScheduleDayOff,
			                                                       RaptorTransformerHelper.CurrentBusinessUnit,
			                                                       _jobParameters.Helper.Repository);
			

			return rowsAffected;
		}
	}
}