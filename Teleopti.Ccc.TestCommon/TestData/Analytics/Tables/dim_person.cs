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
			return table;
		}

		public static void AddPerson(
			this DataTable dataTable,
            int person_id,
            Guid person_code,
            string first_name,
            string last_name,
            DateTime valid_from,
            DateTime valid_to,
            int valid_from_date_id, 
            int valid_from_interval_id,
            int valid_to_date_id,
            int valid_to_interval_id,
			int business_unit_id,
			Guid business_unit_code,
			string business_unit_name,
			int datasource_id,
            bool to_be_deleted)
		{
			var row = dataTable.NewRow();

            row["person_id"] = person_id;
            row["person_code"] = person_code;
            row["valid_from_date"] = valid_from;
            row["valid_to_date"] = valid_to;
		    row["valid_from_date_id"] = valid_from_date_id;
		    row["valid_from_interval_id"] = valid_from_interval_id;
		    row["valid_to_date_id"] = valid_to_date_id;
		    row["valid_to_interval_id"] = valid_to_interval_id;
            row["person_name"] = first_name + last_name;
            row["first_name"] = first_name;
            row["last_name"] = last_name ;
			row["business_unit_id"] = business_unit_id;
			row["business_unit_code"] = business_unit_code;
			row["business_unit_name"] = business_unit_name;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
            row["employment_type_name"] = "";
            row["team_name"] = "";
            row["site_name"] = "";
            row["windows_domain"] = "";
            row["windows_username"] = "";
            row["to_be_deleted"] = to_be_deleted;

			dataTable.Rows.Add(row);
		}
	}
}