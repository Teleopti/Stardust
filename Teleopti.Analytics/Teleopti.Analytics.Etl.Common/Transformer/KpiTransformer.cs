using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class KpiTransformer : IEtlTransformer<IKeyPerformanceIndicator>
	{
		private readonly DateTime _insertDateTime;
		//private readonly DataTable _dataTable = KpiInfrastructure.AddColumnsToDataTable();

		private KpiTransformer() { }

		public KpiTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Transform(IEnumerable<IKeyPerformanceIndicator> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (IKeyPerformanceIndicator kpi in rootList)
			{
				DataRow row = table.NewRow();

				row["kpi_code"] = kpi.Id;
				row["kpi_name"] = kpi.Name;
				row["resource_key"] = kpi.ResourceKey;
				row["target_value_type"] = kpi.TargetValueType;
				row["default_target_value"] = kpi.DefaultTargetValue;
				row["default_min_value"] = kpi.DefaultMinValue;
				row["default_max_value"] = kpi.DefaultMaxValue;
				row["default_between_color"] = kpi.DefaultBetweenColor.ToArgb();
				row["default_lower_than_min_color"] = kpi.DefaultLowerThanMinColor.ToArgb();
				row["default_higher_than_max_color"] = kpi.DefaultHigherThanMaxColor.ToArgb();
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(kpi);

				table.Rows.Add(row);
			}
		}
	}
}
