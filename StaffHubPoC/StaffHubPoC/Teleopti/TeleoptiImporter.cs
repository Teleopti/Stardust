using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using StaffHubPoC.Models;
using StaffHubPoC.StaffHub;
using StaffHubPoC.Types;

namespace StaffHubPoC.Teleopti
{
	public class TeleoptiImporter
	{
		public string ConnectionString;

		public TeleoptiImporter()
		{
			ConnectionString = ConfigurationManager.ConnectionStrings["TeleoptiApp"].ConnectionString;
		}
		public List<TeleoptiShift> GetAllShifts()
		{
			var shifts = new List<TeleoptiShift>();
			using (var conn = new SqlConnection(ConnectionString))
			{
				conn.Open();
				using (var command = new SqlCommand(
				@"SELECT p.Email, sd.BelongsToDate, sd.StartDateTime, sd.EndDateTime, sd.Label 
				from ReadModel.ScheduleDay sd
				inner join Person p on sd.PersonId = p.Id
				where workday = 1", conn))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							shifts.Add(new TeleoptiShift
							{
								Email = reader.GetString(0),
								BelongsToDate = reader.GetDateTime(1),
								StartTime = reader.GetDateTime(2),
								EndTime = reader.GetDateTime(3),
								Label = reader.GetString(4)
							});
						}
					}
				}
			}

			return shifts;
		}
	}
}
