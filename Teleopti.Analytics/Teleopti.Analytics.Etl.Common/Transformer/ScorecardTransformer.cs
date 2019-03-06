using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScorecardTransformer
	{
		private readonly DataTable _dataTable =
			 ScorecardInfrastructure.CreateEmptyDataTable();

		public DataTable Transform(IList<IScorecard> scorecardList, DateTime insertDateTime)
		{
			foreach (IScorecard scorecard in scorecardList)
			{
				DataRow row = _dataTable.NewRow();

				row["scorecard_code"] = scorecard.Id;
				row["scorecard_name"] = scorecard.Name;
				row["period"] = scorecard.Period.Id;
				row["period_name"] = scorecard.Period.Name;
				row["business_unit_code"] = scorecard.GetOrFillWithBusinessUnit_DONTUSE().Id;
				row["business_unit_name"] = scorecard.GetOrFillWithBusinessUnit_DONTUSE().Name;
				//row["business_unit_code"] = RaptorTransformerHelper.CurrentBusinessUnit.Id;
				//row["business_unit_name"] = RaptorTransformerHelper.CurrentBusinessUnit.Description.Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = insertDateTime;
				row["update_date"] = insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(scorecard);

				_dataTable.Rows.Add(row);
			}

			return _dataTable;
		}
	}
}