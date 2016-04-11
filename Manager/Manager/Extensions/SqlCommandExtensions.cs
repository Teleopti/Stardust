using System.Data;
using System.Data.SqlClient;

namespace Stardust.Manager.Extensions
{
	public static class SqlCommandExtensions
	{
		public static string GetCommandToExecute(this SqlCommand command)
		{
			var commandTextToExecute = command.CommandText;

			foreach (SqlParameter p in command.Parameters)
			{
				string parameterValue = p.Value.ToString();

				if (p.SqlDbType == SqlDbType.NVarChar ||
					p.SqlDbType == SqlDbType.UniqueIdentifier ||
					p.SqlDbType == SqlDbType.DateTime)
				{
					parameterValue = "'" + parameterValue + "'";
				}

				commandTextToExecute = commandTextToExecute.Replace(p.ParameterName, parameterValue);
			}

			return commandTextToExecute;
		}
	}
}