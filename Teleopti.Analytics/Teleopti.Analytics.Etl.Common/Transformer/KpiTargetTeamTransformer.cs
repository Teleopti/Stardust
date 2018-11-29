using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class KpiTargetTeamTransformer : IEtlTransformer<IKpiTarget>
	{
		private readonly DateTime _insertDateTime;

		private KpiTargetTeamTransformer() { }

		public KpiTargetTeamTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IKpiTarget> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (IKpiTarget kpiTargetTeam in rootList)
			{
				DataRow row = table.NewRow();

				row["kpi_code"] = kpiTargetTeam.KeyPerformanceIndicator.Id;
				row["team_code"] = kpiTargetTeam.Team.Id;
				row["target_value"] = kpiTargetTeam.TargetValue;
				row["min_value"] = kpiTargetTeam.MinValue;
				row["max_value"] = kpiTargetTeam.MaxValue;
				row["between_color"] = kpiTargetTeam.BetweenColor.ToArgb();
				row["lower_than_min_color"] = kpiTargetTeam.LowerThanMinColor.ToArgb();
				row["higher_than_max_color"] = kpiTargetTeam.HigherThanMaxColor.ToArgb();

				row["business_unit_code"] = kpiTargetTeam.BusinessUnit.Id;
				row["business_unit_name"] = kpiTargetTeam.BusinessUnit.Name;
				//row["business_unit_code"] = RaptorTransformerHelper.CurrentBusinessUnit.Id;
				//row["business_unit_name"] = RaptorTransformerHelper.CurrentBusinessUnit.Description.Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(kpiTargetTeam);

				table.Rows.Add(row);
			}
		}
	}
}
