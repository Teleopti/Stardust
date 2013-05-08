using System;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		"CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public class ContractDataRowFactory
	{
		public static DataRow CreateContractDataRow(DataTable dataTable,
		                                            DateTime date,
		                                            IEntity person,
		                                            IntervalBase interval,
		                                            int contractTime,
		                                            IBusinessUnit businessUnit,
		                                            DateTime insertDateTime)
		{
			var row = dataTable.NewRow();
			row["date"] = date;
			row["person_code"] = person.Id;
			row["interval_id"] = interval.Id;
			row["scheduled_contract_time_m"] = contractTime;

			row["business_unit_code"] = businessUnit.Id;
			row["business_unit_name"] = businessUnit.Name;
			row["datasource_id"] = 1;
			row["insert_date"] = insertDateTime;
			row["update_date"] = insertDateTime;

			return row;
		}
	}
}