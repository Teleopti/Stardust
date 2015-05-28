using System;
using System.Data;
using System.Threading;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class ScorecardInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static DataTable CreateEmptyDataTable()
		{
			DataTable table = new DataTable();
			try
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;

				table.Columns.Add("scorecard_code", typeof(Guid));
				table.Columns.Add("scorecard_name", typeof(string));
				table.Columns.Add("period", typeof(int));
				table.Columns.Add("period_name", typeof(string));
				table.Columns.Add("business_unit_code", typeof(Guid));
				table.Columns.Add("business_unit_name", typeof(String));
				table.Columns.Add("datasource_id", typeof(int));
				table.Columns.Add("insert_date", typeof(DateTime));
				table.Columns.Add("update_date", typeof(DateTime));
				table.Columns.Add("datasource_update_date", typeof(DateTime));
			}
			catch (Exception)
			{
				table.Dispose();
				table = null;
			}

			return table;
		}
	}
}
