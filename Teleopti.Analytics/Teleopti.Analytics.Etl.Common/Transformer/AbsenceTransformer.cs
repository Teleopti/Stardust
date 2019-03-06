using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class AbsenceTransformer : IEtlTransformer<IAbsence>
	{
		private readonly DateTime _insertDateTime;

		private AbsenceTransformer()
		{
		}

		public AbsenceTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IAbsence> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (IAbsence absence in rootList)
			{
				DataRow row = table.NewRow();

				row["absence_code"] = absence.Id;
				row["absence_name"] = absence.Description.Name;
				row["absence_shortname"] = absence.Description.ShortName;
				row["display_color"] = absence.DisplayColor.ToArgb();
				row["display_color_html"] = ColorTranslator.ToHtml(absence.DisplayColor);
				row["business_unit_code"] = absence.GetOrFillWithBusinessUnit_DONTUSE().Id;
				row["business_unit_name"] = absence.GetOrFillWithBusinessUnit_DONTUSE().Name;

				row["in_contract_time"] = absence.InContractTime;
				row["in_paid_time"] = absence.InPaidTime;
				row["in_work_time"] = absence.InWorkTime;

				//row["business_unit_code"] = RaptorTransformerHelper.CurrentBusinessUnit.Id;
				//row["business_unit_name"] = RaptorTransformerHelper.CurrentBusinessUnit.Description.Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(absence);

				IDeleteTag absenceDeleteCheck = absence as IDeleteTag;
				if (absenceDeleteCheck != null)
				{
					row["is_deleted"] = absenceDeleteCheck.IsDeleted;
				}

				table.Rows.Add(row);
			}
		}


	}
}
