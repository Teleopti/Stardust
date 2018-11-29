using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ActivityTransformer : IEtlTransformer<IActivity>
	{
		private readonly DateTime _insertDateTime;
		private ActivityTransformer() { }

		public ActivityTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IActivity> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (var activity in rootList)
			{
				DataRow row = table.NewRow();

				row["activity_code"] = activity.Id;
				row["activity_name"] = activity.Description.Name;
				row["display_color"] = activity.DisplayColor.ToArgb();
				row["display_color_html"] = ColorTranslator.ToHtml(activity.DisplayColor);
				row["in_ready_time"] = activity.InReadyTime;
				row["in_contract_time"] = activity.InContractTime;
				row["in_paid_time"] = activity.InPaidTime;
				row["in_work_time"] = activity.InWorkTime;
				row["business_unit_code"] = activity.BusinessUnit.Id;
				row["business_unit_name"] = activity.BusinessUnit.Name;
				//row["business_unit_code"] = RaptorTransformerHelper.CurrentBusinessUnit.Id;
				//row["business_unit_name"] = RaptorTransformerHelper.CurrentBusinessUnit.Description.Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(activity);
				row["is_deleted"] = activity.IsDeleted;

				table.Rows.Add(row);
			}
		}
	}
}
