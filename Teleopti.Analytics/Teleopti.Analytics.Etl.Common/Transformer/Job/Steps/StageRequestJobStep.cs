using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageRequestJobStep : JobStepBase
	{
		public StageRequestJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_request";
			Transformer = new RequestTransformer();
			JobCategory = JobCategoryType.Schedule;
			RequestInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		//public IEtlTransformer<IPersonRequest> Transformer { get; set; }
		public IPersonRequestTransformer<IPersonRequest> Transformer { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			// 1. get the data from repository.
			// 2. transfrom data to the data table.
			// 3. Bulk insert that data table to database.
			// 4. return effected rows.

			// this steps moves data from CCC7 to Stage.
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor,
																	JobCategoryDatePeriod.EndDateUtcCeiling);

			var rootList = _jobParameters.Helper.Repository.LoadRequest(period);
			Transformer.Transform(rootList, _jobParameters.IntervalsPerDay, BulkInsertDataTable1);

			return _jobParameters.Helper.Repository.PersistRequest(BulkInsertDataTable1);
		}
	}
}