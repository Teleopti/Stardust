using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using StaffHubPoC.Models;
using StaffHubPoC.Types;

namespace StaffHubPoC.Teleopti
{
	public class TeleoptiImporter
	{
		public string ConnectionString;

		public TeleoptiImporter()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["TeleoptiAnalytics"].ConnectionString;
		}
		public List<TeleoptiShift> GetAllShifts()
		{
			var shifts = new List<TeleoptiShift>();
			using (var conn = new SqlConnection(ConnectionString))
			{
				conn.Open();
				using (var command = new SqlCommand(
				@"SELECT p.email, shift_starttime, shift_endtime, sc.shift_category_name, ab.absence_name
  FROM [mart].[fact_schedule] s
  inner join mart.dim_person p on s.person_id=p.person_id
  inner join mart.fact_schedule_day_count sdc on s.shift_starttime=sdc.starttime and s.person_id=sdc.person_id
  inner join mart.dim_shift_category sc on sdc.shift_category_id=sc.shift_category_id
  inner join mart.dim_absence ab on ab.absence_id=s.absence_id
  group by shift_starttime, shift_endtime, p.email, sc.shift_category_name, ab.absence_name
  order by s.shift_starttime", conn))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							shifts.Add(new TeleoptiShift
							{
								Email = reader.GetString(0),
								StartTime = reader.GetDateTime(1),
								EndTime = reader.GetDateTime(2),
								Label = reader.GetString(3) != "Not Defined" ? reader.GetString(3) : reader.GetString(4),
								Breaks = new List<Break>(),
								Working = reader.GetString(4) == "Not Defined"
							});
						}
					}
				}

				using (var command = new SqlCommand(
					@"  select p.email, a.activity_name, a.in_paid_time, Datediff(MINUTE, s.activity_starttime, s.activity_endtime), s.shift_starttime from mart.fact_schedule s
				inner join mart.dim_activity a on s.activity_id=a.activity_id
				inner join mart.dim_person p on s.person_id=p.person_id
				where a.in_work_time = 0", conn))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var shift = shifts.First(x => x.Email == reader.GetString(0) && x.StartTime == reader.GetDateTime(4));
							shift.Breaks.Add(new Break
							{
								breakType = reader.GetSqlBoolean(2).IsTrue ? BreakType.Paid : BreakType.Unpaid,
								duration = reader.GetInt32(3)
						});
							
						}
					}
				}
			}

			return shifts;
		}
	}
}
