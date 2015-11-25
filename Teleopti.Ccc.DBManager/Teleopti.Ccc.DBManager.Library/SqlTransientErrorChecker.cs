using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.DBManager.Library
{
	public class SqlTransientErrorChecker
	{
		public bool IsTransient(Exception ex)
		{
			if (ex != null)
			{
				SqlException sqlException;
				if ((sqlException = ex as SqlException) != null)
				{
					// Enumerate through all errors found in the exception.
					foreach (SqlError err in sqlException.Errors)
					{
						switch (err.Number)
						{
							case -2: //Timeout!
							case 64:
							case 233:
							case 4060:
							case 10053:
							case 10054:
							case 10060:
							case 10928:
							case 10929:
							case 40501:
							case 40143:
							case 40197:
							case 40540:
							case 40613:
							case 40648:
							case 40671:
							case 42019:
							case 45168:
							case 45169:
							case 49918:
							case 49919:
							case 49920:
								return true;
						}
					}
				}
				else if (ex is TimeoutException)
				{
					return true;
				}
			}

			return false;
		}
	}
}