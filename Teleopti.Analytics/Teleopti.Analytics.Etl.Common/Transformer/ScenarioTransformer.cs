using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScenarioTransformer : IEtlTransformer<IScenario>
	{
		private readonly DateTime _insertDateTime;

		public ScenarioTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		private ScenarioTransformer() { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IScenario> rootList, DataTable table)
		{
			foreach (IScenario scenario in rootList)
			{
				DataRow row = table.NewRow();

				row["scenario_code"] = scenario.Id;
				row["scenario_name"] = scenario.Description.Name;
				row["default_scenario"] = scenario.DefaultScenario;
				row["business_unit_code"] = scenario.GetOrFillWithBusinessUnit_DONTUSE().Id;
				row["business_unit_name"] = scenario.GetOrFillWithBusinessUnit_DONTUSE().Name;
				//row["business_unit_code"] = RaptorTransformerHelper.CurrentBusinessUnit.Id;
				//row["business_unit_name"] = RaptorTransformerHelper.CurrentBusinessUnit.Description.Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(scenario);

				IDeleteTag scenarioDeleteCheck = scenario as IDeleteTag;
				if (scenarioDeleteCheck != null)
				{
					row["is_deleted"] = scenarioDeleteCheck.IsDeleted;
				}

				table.Rows.Add(row);
			}
		}
	}
}
