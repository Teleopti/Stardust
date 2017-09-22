using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageKpiTargetTeamJobStep : JobStepBase
	{
		public StageKpiTargetTeamJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_kpi_targets_team";
			KpiTargetTeamInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IKpiTarget> rootList = _jobParameters.Helper.Repository.LoadKpiTargetTeam();

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new KpiTargetTeamTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistKpiTargetTeam(BulkInsertDataTable1);
		}
	}
}
