using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_person
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_person");
			table.Columns.Add("person_id", typeof(int));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("valid_from_date", typeof(DateTime));
			table.Columns.Add("valid_to_date", typeof(DateTime));
			table.Columns.Add("valid_from_date_id", typeof(int));
			table.Columns.Add("valid_from_interval_id", typeof(int));
			table.Columns.Add("valid_to_date_id", typeof(int));
			table.Columns.Add("valid_to_interval_id", typeof(int));
			table.Columns.Add("person_period_code", typeof(Guid));
			table.Columns.Add("person_name", typeof(String));
			table.Columns.Add("first_name", typeof(String));
			table.Columns.Add("last_name", typeof(String));
			table.Columns.Add("employment_number", typeof(String));
			table.Columns.Add("employment_type_code", typeof(Guid));
			table.Columns.Add("employment_type_name", typeof(String));
			table.Columns.Add("contract_code", typeof(Guid));
			table.Columns.Add("contract_name", typeof(String));
			table.Columns.Add("parttime_code", typeof(Guid));
			table.Columns.Add("parttime_percentage", typeof(String));
			table.Columns.Add("team_id", typeof(int));
			table.Columns.Add("team_code", typeof(Guid));
			table.Columns.Add("team_name", typeof(String));
			table.Columns.Add("site_id", typeof(int));
			table.Columns.Add("site_code", typeof(Guid));
			table.Columns.Add("site_name", typeof(String));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("skillset_id", typeof(int));
			table.Columns.Add("email", typeof(String));
			table.Columns.Add("note", typeof(String));
			table.Columns.Add("employment_start_date", typeof(DateTime));
			table.Columns.Add("employment_end_date", typeof(DateTime));
			table.Columns.Add("time_zone_id", typeof(String));
			table.Columns.Add("is_agent", typeof(bool));
			table.Columns.Add("is_user", typeof(bool));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("to_be_deleted", typeof(int));
			table.Columns.Add("windows_domain", typeof(String));
			table.Columns.Add("windows_username", typeof(String));
			table.Columns.Add("valid_to_date_id_maxDate", typeof(int));
			table.Columns.Add("valid_to_interval_id_maxDate", typeof(int));
			table.Columns.Add("valid_from_date_id_local", typeof(int));
			table.Columns.Add("valid_to_date_id_local", typeof(int));
			table.Columns.Add("valid_from_date_local", typeof(DateTime));
			table.Columns.Add("valid_to_date_local", typeof(DateTime));
			return table;
		}

		public static void AddPerson(
			this DataTable dataTable,
			int personId,
			Guid personCode,
			string firstName,
			string lastName,
			DateTime validFrom,
			DateTime validTo,
			int validFromDateId, 
			int validFromIntervalId,
			int validToDateId,
			int validToIntervalId,
			int businessUnitId,
			Guid businessUnitCode,
			string businessUnitName,
			int datasourceId,
			bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["person_id"] = personId;
			row["person_code"] = personCode;
			row["valid_from_date"] = validFrom;
			row["valid_to_date"] = validTo;
			row["valid_from_date_id"] = validFromDateId;
			row["valid_from_interval_id"] = validFromIntervalId;
			row["valid_to_date_id"] = validToDateId;
			row["valid_to_interval_id"] = validToIntervalId;
			row["person_name"] = firstName + lastName;
			row["first_name"] = firstName;
			row["last_name"] = lastName ;
			row["business_unit_id"] = businessUnitId;
			row["business_unit_code"] = businessUnitCode;
			row["business_unit_name"] = businessUnitName;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["employment_type_name"] = "";
			row["team_name"] = "";
			row["site_name"] = "";
			row["windows_domain"] = "";
			row["windows_username"] = "";
			row["to_be_deleted"] = toBeDeleted;
			row["valid_to_date_id_maxDate"] = validToDateId;
			row["valid_to_interval_id_maxDate"] = 0;
			row["valid_from_date_id_local"]= validFromDateId;
			row["valid_to_date_id_local"]= validToDateId;
			row["valid_from_date_local"]= validFrom;
			row["valid_to_date_local"]  = validTo;


			dataTable.Rows.Add(row);
		}
	}
}