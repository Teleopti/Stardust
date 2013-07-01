using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CheckPreRequisites.Checks
{
	public class V6Check
	{
		private readonly DatabaseCheck _databaseCheck;
		private readonly Form1 _form1;

		public V6Check(Form1 form1, DatabaseCheck databaseCheck)
		{
			_form1 = form1;
			_databaseCheck = databaseCheck;
		}

		public void RunV6Checks(string dbName)
		{
			var dbConnection = false;
			var connectionString = _form1.ConnStringGet(dbName);

			//Check that it's possible to connect
			_databaseCheck.DBConnectionCheck(ref dbConnection, connectionString, dbName);

			//if we have a Db-connection, go for the DB-checks
			if (!dbConnection) return;
			InsertProcedure(connectionString);
			DbPreMigrationCheck(connectionString);
			DbPreMigrationAbsenceTable(connectionString);
			DbPreMigrationActivitiesTable(connectionString);
		}



		private static void InsertProcedure(string connString)
		{
			var regex = new Regex("^\\s*GO\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			// create a writer and open the file
			const string filePath = @"p_raptor_run_before_conversion.sql";
			using (var conn = new SqlConnection(connString))
			using (var cmd = new SqlCommand())
			{
				conn.Open();
				cmd.Connection = conn;
				if (File.Exists(filePath))
				{
					StreamReader file = null;
					try
					{
						file = new StreamReader(filePath);
						var lines = regex.Split(file.ReadToEnd());

						foreach (var line in lines)
						{
							if (line.Length <= 0) continue;

							cmd.CommandText = line;
							cmd.CommandType = CommandType.Text;
							cmd.NotificationAutoEnlist = true;
							cmd.ExecuteNonQuery();
						}
					}
					finally
					{
						if (file != null)
							file.Close();
					}
				}
				conn.Close();
			}
		}

		private void DbPreMigrationCheck(string connString)
		{
			const string sysAdminCommand = "exec [dbo].[p_raptor_run_before_conversion]";
			_form1.printNewFeature("Database V6 Check", "", "", "");

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(sysAdminCommand, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							_form1.printNewFeature(reader[0].ToString(), reader[1].ToString(), "", reader[2].ToString());
							_form1.printFeatureStatus(false);
						}
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					_form1.printNewFeature("Error", "Database", "", ex.Message);
					_form1.printFeatureStatus(false);
				}
			}
		}

		private void DbPreMigrationAbsenceTable(string connString)
		{
			const string commandText1 =
				"SELECT [abs_id],[abs_desc],[abs_desc_nonuni],[abs_short_desc],[abs_short_desc_nonuni],[extended_abs],[color_code],[apply_time_rules],[apply_count_rules],[activity_id],[deleted],[time_tracking],[in_worktime],[paid_time],[block_delimiter],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[absences]";
			var rowList = new ArrayList();

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(commandText1, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var values = new object[reader.FieldCount];
							reader.GetValues(values);
							rowList.Add(values);
						}
					}

					_form1.listView2.Items.Clear();
					foreach (object[] row in rowList)
					{
						var orderDetails = new string[row.Length];
						var columnIndex = 0;

						foreach (var column in row)
						{
							orderDetails[columnIndex++] = Convert.ToString(column);
						}

						var newItem = new ListViewItem(orderDetails);
						_form1.listView2.Items.Add(newItem);
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					_form1.printNewFeature("Error", "Database", "", ex.Message);
					_form1.printFeatureStatus(false);
				}
			}
		}

		private void DbPreMigrationActivitiesTable(string connString)
		{
			const string commandText1 =
				"SELECT [activity_id],[activity_name],[activity_name_nonuni],[in_worktime],[color_code],[occupies_workplace],[show_in_schedule],[extended_activity],[in_shiftname],[ep_lunch_break],[ep_short_break],[is_logged],[req_skill],[is_parent],[parent_id],[deleted],[overwrite],[paid_time],[planned_absence],[unplanned_absence],[vacation],[private_desc],[private_color],[changed_by],[changed_date] FROM [dbo].[activities]";
			var rowList = new ArrayList();

			using (var conn = new SqlConnection(connString))
			{
				try
				{
					conn.Open();
					using (var cmd = new SqlCommand(commandText1, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var values = new object[reader.FieldCount];
							reader.GetValues(values);
							rowList.Add(values);
						}
					}

					_form1.listView3.Items.Clear();
					foreach (object[] row in rowList)
					{
						var orderDetails = new string[row.Length];
						var columnIndex = 0;

						foreach (var column in row)
							orderDetails[columnIndex++] = Convert.ToString(column);

						var newItem = new ListViewItem(orderDetails);
						_form1.listView3.Items.Add(newItem);
					}
					conn.Close();
				}
				catch (Exception ex)
				{
					_form1.printNewFeature("Error", "Database", "", ex.Message);
					_form1.printFeatureStatus(false);
				}
			}
		}
	}
}
