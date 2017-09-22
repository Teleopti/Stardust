using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageWorkloadJobStep : JobStepBase
	{
		public StageWorkloadJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_workload, stg_queue_workload";
			WorkloadInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
			WorkloadQueueInfrastructure.AddColumnsToDataTable(BulkInsertDataTable2);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IWorkload> rootList = _jobParameters.Helper.Repository.LoadWorkload();

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new WorkloadTransformer(DateTime.Now);

			raptorTransformer.Transform(rootList, BulkInsertDataTable1, BulkInsertDataTable2);

			int affectedWorkloadRows = _jobParameters.Helper.Repository.PersistWorkload(BulkInsertDataTable1);
			int affectedWorkloadQueueTable = _jobParameters.Helper.Repository.PersistQueueWorkload(BulkInsertDataTable2);

			return affectedWorkloadRows + affectedWorkloadQueueTable;
		}
	}
}