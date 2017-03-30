using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageGroupPagePersonJobStep : JobStepBase
	{
		public StageGroupPagePersonJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_group_page_person";
			Transformer = new GroupPagePersonTransformer(() => _jobParameters.StateHolder, _jobParameters.ToggleManager);
			GroupPagePersonInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IGroupPagePersonTransformer Transformer { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			IList<IGroupPage> builtInGroupPages = Transformer.BuiltInGroupPages;
			IEnumerable<IGroupPage> userDefinedGroupings = Transformer.UserDefinedGroupings;
			Transformer.Transform(builtInGroupPages, userDefinedGroupings, BulkInsertDataTable1);

			return _jobParameters.Helper.Repository.PersistGroupPagePerson(BulkInsertDataTable1);
		}
	}
}
