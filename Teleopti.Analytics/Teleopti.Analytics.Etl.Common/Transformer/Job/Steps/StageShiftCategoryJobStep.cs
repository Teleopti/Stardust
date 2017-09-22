using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageShiftCategoryJobStep : JobStepBase
	{
		public StageShiftCategoryJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_shift_category";
			ShiftCategoryInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IShiftCategory> rootList = _jobParameters.Helper.Repository.LoadShiftCategory();

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new ShiftCategoryTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistShiftCategory(BulkInsertDataTable1);
		}
	}
}