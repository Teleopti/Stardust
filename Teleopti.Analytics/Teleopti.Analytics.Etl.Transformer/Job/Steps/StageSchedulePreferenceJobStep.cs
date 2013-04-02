﻿using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	/// <summary>
	/// Responsible for retriving the PersonRestrictions  and send it to the transformer
	/// </summary>
	/// <remarks>
	/// Created by: henrika
	/// Created date: 2009-07-02
	/// </remarks>
	public class StageSchedulePreferenceJobStep : JobStepBase
	{
		private readonly INeedToRunChecker _needToRunChecker;

		public StageSchedulePreferenceJobStep(IJobParameters jobParameters)
			: this(jobParameters, new DefaultNeedToRunChecker())
		{}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public StageSchedulePreferenceJobStep(IJobParameters jobParameters, INeedToRunChecker needToRunChecker)
			: base(jobParameters)
		{
			_needToRunChecker = needToRunChecker;
			Name = "stg_schedule_preference, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new SchedulePreferenceTransformer(_jobParameters.IntervalsPerDay);
			ScheduleDayRestrictor = new ScheduleDayRestrictor();
			DayOffSubStep = new EtlDayOffSubStep();
			SchedulePreferenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public ISchedulePreferenceTransformer Transformer { get; set; }
		public IScheduleDayRestrictor ScheduleDayRestrictor { get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			//IScenario scenario = _jobParameters.StateHolder.DefaultScenario;

			if (!_needToRunChecker.NeedToRun(period, _jobParameters.Helper.Repository, Result.CurrentBusinessUnit, Name))
			{
				//gets resetted to Done later :(
				Result.Status = "No need to run";
				return 0;
			}

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				IList<IScheduleDay> scheduleParts = _jobParameters.StateHolder.LoadSchedulePartsPerPersonAndDate(period, scenario);
				//Remove parts ending too late. This because the schedule repository fetches restrictions on larger period than schedule.
				scheduleParts = ScheduleDayRestrictor.RemoveScheduleDayEndingTooLate(scheduleParts, period.EndDateTime);
				Transformer.Transform(scheduleParts, BulkInsertDataTable1);
			}

			int rowsAffected = _jobParameters.Helper.Repository.PersistSchedulePreferences(BulkInsertDataTable1);
			
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.SchedulePreference,
			                                                       RaptorTransformerHelper.CurrentBusinessUnit,
			                                                       _jobParameters.Helper.Repository);

			return rowsAffected;
		}
	}
}
