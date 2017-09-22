using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageSkillJobStep : JobStepBase
	{
		public StageSkillJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_skill";
			SkillInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<ISkill> rootList = _jobParameters.StateHolder.SkillCollection;

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new SkillTransformer(DateTime.Now);
			raptorTransformer.Transform(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistSkill(BulkInsertDataTable1);

		}
	}
}