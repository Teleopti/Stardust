using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageAbsenceJobStep : JobStepBase
	{
		public StageAbsenceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_absence";
			AbsenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IAbsence> rootList = _jobParameters.Helper.Repository.LoadAbsence();

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new AbsenceTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistAbsence(BulkInsertDataTable1);
		}
	}
}