

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
	public interface IPersonAssignmentDateResetter
	{
		bool ExecuteFor(IPerson person, string connectionString);
	}

	public class PersonAssignmentDateResetter : IPersonAssignmentDateResetter
	{

		private readonly PersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		public bool ExecuteFor(IPerson person, string connectionString)
		{

			using (var connection = new SqlConnection(connectionString))
			{
				IList<DataRow> rows;
				connection.Open();
				using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					rows = _personAssignmentCommon.ReadRows(connection, readCommandForAssignments(person), transaction);
					setFields(rows);
					updatePersonAssignmentRows(rows, connection, transaction);

					rows = _personAssignmentCommon.ReadRows(connection, readCommandForAssignmentsAudit(person), transaction);
					setFields(rows);
					updatePersonAssignmentAuditRows(rows, connection, transaction);

					transaction.Commit();
				}
				connection.Close();
			}


			return true;

		}

		private void setFields(IEnumerable<DataRow> rows)
		{
			foreach (var dataRow in rows)
			{
				dataRow["TheDate"] = new DateTime(1800, 1, 1);
				var version = dataRow.Field<int>("Version");
				dataRow["Version"] = version + 1;
			}
		}

		private static void updatePersonAssignmentRows(IEnumerable<DataRow> rows, SqlConnection connection, SqlTransaction transaction)
		{
			using (var command = new SqlCommand())
			{
				command.CommandType = CommandType.Text;
				command.Connection = connection;
				command.Transaction = transaction;
				foreach (var dataRow in rows)
				{
					var commandText = new StringBuilder()
						.AppendLine("update dbo.PersonAssignment")
						.AppendLine("set TheDate = '" + dataRow.Field<DateTime>("TheDate") + "'")
						.AppendLine(", Version = " + dataRow.Field<int>("Version"))
						.AppendLine("where Id='" + dataRow.Field<Guid>("Id") + "'");

					command.CommandText = commandText.ToString();
					command.ExecuteNonQuery();
				}
			}
		}

		private static void updatePersonAssignmentAuditRows(IEnumerable<DataRow> rows, SqlConnection connection, SqlTransaction transaction)
		{
			using (var command = new SqlCommand())
			{
				command.CommandType = CommandType.Text;
				command.Connection = connection;
				command.Transaction = transaction;
				foreach (var dataRow in rows)
				{
					var commandText = new StringBuilder()
						.AppendLine("update [Auditing].PersonAssignment_AUD")
						.AppendLine("set TheDate = '" + dataRow.Field<DateTime>("TheDate") + "'")
						.AppendLine(", Version = " + dataRow.Field<int>("Version"))
						.AppendLine("where Id='" + dataRow.Field<Guid>("Id") + "'");

					command.CommandText = commandText.ToString();
					command.ExecuteNonQuery();
				}
			}
		}

		private string readCommandForAssignments(IPerson person)
		{
			return new StringBuilder()
				.AppendLine("select")
				.AppendLine("Pa.Id, TheDate, Pa.Version")
				.AppendLine("from dbo.PersonAssignment pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("where TheDate > '1800-01-01'")
				.AppendLine("and p.id = " + person.Id)
				.ToString();
		}

		private string readCommandForAssignmentsAudit(IPerson person)
		{
			return new StringBuilder()
				.AppendLine("select")
				.AppendLine("Pa.Id, TheDate, Pa.Version")
				.AppendLine("from [Auditing].PersonAssignment_AUD pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("where TheDate > '1800-01-01'")
				.AppendLine("and p.id = " + person.Id)
				.ToString();
		}

		
	}
}