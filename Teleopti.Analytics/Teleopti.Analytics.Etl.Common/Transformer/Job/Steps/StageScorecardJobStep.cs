using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageScorecardJobStep : JobStepBase
	{
		public StageScorecardJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_scorecard";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IList<IScorecard> rootList = _jobParameters.Helper.Repository.LoadScorecard();

			//Transform data from Raptor to Matrix format
			var raptorTransformer = new ScorecardTransformer();
			DataTable bulkTable = raptorTransformer.Transform(rootList, DateTime.Now);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistScorecard(bulkTable);
		}
	}
}