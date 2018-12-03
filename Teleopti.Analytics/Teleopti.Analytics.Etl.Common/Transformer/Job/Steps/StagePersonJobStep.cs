using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StagePersonJobStep : JobStepBase
	{
		public StagePersonJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_person";
			PersonInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
			AcdLogOnPersonInfrastructure.AddColumnsToDataTable(BulkInsertDataTable2);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get data from Raptor
			IEnumerable<IPerson> rootList = _jobParameters.StateHolder.PersonCollection.ToList();
			var guids = rootList.Select(person => person.Id.GetValueOrDefault()).ToList();
			var logonInfos = JobParameters.TenantLogonInfoLoader.GetLogonInfoModelsForGuids(guids);
			// Get common agent name description setting
			ICommonNameDescriptionSetting commonNameDescriptionSetting = _jobParameters.Helper.Repository.CommonAgentNameDescriptionSetting;

			

			//Transform data from Raptor to Matrix format
			PersonTransformer.Transform(rootList, 
											_jobParameters.IntervalsPerDay,
											DateOnly.Today,
											BulkInsertDataTable1,
											BulkInsertDataTable2,
											commonNameDescriptionSetting,
											logonInfos.ToList());

			int affectedPersonRows = _jobParameters.Helper.Repository.PersistPerson(BulkInsertDataTable1);
			int affectedAcdLoginRows = _jobParameters.Helper.Repository.PersistAcdLogOnPerson(BulkInsertDataTable2);

			return affectedPersonRows + affectedAcdLoginRows;
		}

	}
}