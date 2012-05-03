﻿using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class StageOvertimeJobStep : JobStepBase
	{
		public StageOvertimeJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_overtime";
			OvertimeInfrastructure.AddColumnsToTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IMultiplicatorDefinitionSet> rootList = _jobParameters.StateHolder.MultiplicatorDefinitionSetCollection;

			//Transform data from Raptor to Matrix format
			OvertimeTransformer raptorTransformer = new OvertimeTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistOvertime(BulkInsertDataTable1);
		}
	}
}
