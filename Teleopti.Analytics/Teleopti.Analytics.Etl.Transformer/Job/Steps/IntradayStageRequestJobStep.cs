using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class IntradayStageRequestJobStep : JobStepBase
	{
		public IntradayStageRequestJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_request";
			Transformer = new RequestTransformer();
			JobCategory = JobCategoryType.Schedule;
			RequestInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IPersonRequestTransformer<IPersonRequest> Transformer { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//var rowsAffected = 0;

			// Find any changes since last ETL, and get those (only)
			//var rep = _jobParameters.Helper.Repository;
			var etlIntraday = _jobParameters.Helper.Repository.LastChangedDate(Result.CurrentBusinessUnit, "Requests");
			
			_jobParameters.StateHolder.SetThisTime(etlIntraday, "Requests");

			if (etlIntraday.LastTime == etlIntraday.ThisTime)
			{
				_jobParameters.Helper.Repository.TruncateRequest();
				return 0;
			}
			
			var startTimeUtc = DateTime.SpecifyKind(etlIntraday.LastTime, DateTimeKind.Utc);

			//TODO: Get only changed persons
			ICollection<IPerson> person = _jobParameters.StateHolder.PersonCollection.ToList();

			var rootList = _jobParameters.Helper.Repository.LoadIntradayRequest(person, startTimeUtc);
			Transformer.Transform(rootList, _jobParameters.IntervalsPerDay, BulkInsertDataTable1);

			return _jobParameters.Helper.Repository.PersistRequest(BulkInsertDataTable1);
		}
	}
}