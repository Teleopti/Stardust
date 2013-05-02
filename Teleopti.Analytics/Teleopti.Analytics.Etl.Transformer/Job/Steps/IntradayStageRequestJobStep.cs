using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

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
			var rep = _jobParameters.Helper.Repository;
			var ETLIntraday = _jobParameters.Helper.Repository.LastChangedDate(Result.CurrentBusinessUnit, "Requests");
			
			_jobParameters.StateHolder.SetThisTime(ETLIntraday, "Requests");

			if (ETLIntraday.LastTime == ETLIntraday.ThisTime)
				return 0;
			else
			{
				var startTimeUTC = DateTime.SpecifyKind(ETLIntraday.LastTime, DateTimeKind.Utc);
				var endTimeUTC = DateTime.SpecifyKind(ETLIntraday.ThisTime, DateTimeKind.Utc);
				ICollection<IPerson> person = _jobParameters.StateHolder.PersonCollection.ToList();

				DateTimePeriod period = new DateTimePeriod(startTimeUTC, endTimeUTC);
				var rootList = _jobParameters.Helper.Repository.LoadIntradayRequest(person, period);
				Transformer.Transform(rootList, _jobParameters.IntervalsPerDay, BulkInsertDataTable1);

				return _jobParameters.Helper.Repository.PersistRequest(BulkInsertDataTable1);
			}
		}
	}
}