using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageStateGroupJobStep : JobStepBase
	{
		public StageStateGroupJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_state_group";
			IsBusinessUnitIndependent = true;
			StateGroupInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			var rootList = _jobParameters.Helper.Repository.LoadRtaStateGroups(RaptorTransformerHelper.CurrentBusinessUnit);

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new StateGroupTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistStateGroup(BulkInsertDataTable1);
		}
	}
}