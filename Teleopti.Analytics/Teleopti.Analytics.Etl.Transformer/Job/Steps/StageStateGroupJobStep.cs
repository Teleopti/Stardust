using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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
			StateGroupTransformer raptorTransformer = new StateGroupTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return  _jobParameters.Helper.Repository.PersistStateGroup(BulkInsertDataTable1);
        }
    }
}