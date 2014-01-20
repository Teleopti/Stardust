using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class StateGroupTransformer
    {
        private readonly DateTime _insertDateTime;

		private StateGroupTransformer() { }

        public StateGroupTransformer(DateTime insertDateTime)
            : this()
        {
            _insertDateTime = insertDateTime;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IList<IRtaStateGroup> rootList, DataTable stateGroupTable)
        {
            InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("stateGroupTable", stateGroupTable);

			foreach (IRtaStateGroup stateGroup in rootList)
            {
				DataRow dataRow = stateGroupTable.NewRow();

				dataRow["state_group_code"] = stateGroup.Id;
				dataRow["state_group_name"] = stateGroup.Name;
				dataRow["business_unit_code"] = stateGroup.BusinessUnit.Id;
                dataRow["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
                dataRow["insert_date"] = _insertDateTime;
                dataRow["update_date"] = _insertDateTime;
				dataRow["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(stateGroup);

				IDeleteTag deleteCheck = stateGroup as IDeleteTag;
                if (deleteCheck != null)
                {
                    dataRow["is_deleted"] = deleteCheck.IsDeleted;
                }

                dataRow["is_log_out_state"] = stateGroup.IsLogOutState;

				stateGroupTable.Rows.Add(dataRow);
            }
        }
    }
}
