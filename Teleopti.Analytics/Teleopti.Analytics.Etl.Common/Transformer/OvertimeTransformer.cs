using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class OvertimeTransformer
	{
		private readonly DateTime _insertDateTime;

		private OvertimeTransformer() { }

		public OvertimeTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		public void Transform(IList<IMultiplicatorDefinitionSet> rootList, DataTable multiplicatorDefinitionSetTable)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("skillTable", multiplicatorDefinitionSetTable);

			foreach (IMultiplicatorDefinitionSet multiplicatorDefinitionSet in rootList)
			{
				if (multiplicatorDefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
				{
					DataRow dataRow = multiplicatorDefinitionSetTable.NewRow();

					dataRow["overtime_code"] = multiplicatorDefinitionSet.Id;
					dataRow["overtime_name"] = multiplicatorDefinitionSet.Name;
					dataRow["business_unit_code"] = multiplicatorDefinitionSet.GetOrFillWithBusinessUnit_DONTUSE().Id;
					dataRow["business_unit_name"] = multiplicatorDefinitionSet.GetOrFillWithBusinessUnit_DONTUSE().Name;
					dataRow["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
					dataRow["insert_date"] = _insertDateTime;
					dataRow["update_date"] = _insertDateTime;
					dataRow["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(multiplicatorDefinitionSet);

					var deleteCheck = multiplicatorDefinitionSet as IDeleteTag;
					if (deleteCheck != null)
					{
						dataRow["is_deleted"] = deleteCheck.IsDeleted;
					}

					multiplicatorDefinitionSetTable.Rows.Add(dataRow);
				}
			}
		}
	}
}
