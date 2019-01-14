using System;
using System.Data;
using System.Globalization;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public static class SqlCommandFactory
	{
		public static IDbCommand CreateCommand(IDbConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			IDbCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandTimeout = 60;
			return command;
		}

		public static IDbCommand CreateCommand(IDbConnection connection, string commandText)
		{
			if (commandText == null)
				throw new ArgumentNullException(nameof(commandText));
			if (string.IsNullOrWhiteSpace(commandText))
				throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "StringCannotBeEmpty", new object[1]
				{
					(object) nameof (commandText)
				}), nameof(commandText));
			IDbCommand command = SqlCommandFactory.CreateCommand(connection);
			try
			{
				command.CommandText = commandText;
				return command;
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}

		public static IDbCommand CreateGetContextInfoCommand(IDbConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			IDbCommand command = SqlCommandFactory.CreateCommand(connection);
			try
			{
				command.CommandType = CommandType.Text;
				command.CommandText = "SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(36), CONTEXT_INFO()))";
				return command;
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}
	}
}