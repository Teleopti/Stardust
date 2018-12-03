using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScorecardKpiTransformer : IEtlTransformer<IScorecard>
	{
		private readonly DateTime _insertDateTime;

		private ScorecardKpiTransformer() { }

		public ScorecardKpiTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IScorecard> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (IScorecard scorecard in rootList)
			{
				ReadOnlyCollection<IKeyPerformanceIndicator> kpiCollection = scorecard.KeyPerformanceIndicatorCollection;
				foreach (IKeyPerformanceIndicator kpi in kpiCollection)
				{
					DataRow row = table.NewRow();

					row["scorecard_code"] = scorecard.Id;
					row["kpi_code"] = kpi.Id;

					row["business_unit_code"] = scorecard.BusinessUnit.Id;
					row["business_unit_name"] = scorecard.BusinessUnit.Name;
					row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
					row["insert_date"] = _insertDateTime;
					row["update_date"] = _insertDateTime;
					row["datasource_update_date"] = scorecard.UpdatedOn;

					table.Rows.Add(row);
				}
			}
		}
	}
}
