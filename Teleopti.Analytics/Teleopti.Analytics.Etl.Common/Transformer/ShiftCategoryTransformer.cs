using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ShiftCategoryTransformer : IEtlTransformer<IShiftCategory>
	{
		private readonly DateTime _insertDateTime;

		private ShiftCategoryTransformer() { }

		public ShiftCategoryTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IShiftCategory> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (IShiftCategory shiftCategory in rootList)
			{
				DataRow row = table.NewRow();

				row["shift_category_code"] = shiftCategory.Id;
				row["shift_category_name"] = shiftCategory.Description.Name;
				row["shift_category_short_name"] = shiftCategory.Description.ShortName;
				row["display_color"] = shiftCategory.DisplayColor.ToArgb();
				row["business_unit_code"] = shiftCategory.GetOrFillWithBusinessUnit_DONTUSE().Id;
				row["business_unit_name"] = shiftCategory.GetOrFillWithBusinessUnit_DONTUSE().Name;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;
				row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(shiftCategory);
				row["is_deleted"] = ((IDeleteTag)shiftCategory).IsDeleted;

				table.Rows.Add(row);
			}
		}
	}
}
