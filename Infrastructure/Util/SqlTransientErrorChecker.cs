﻿using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.Util
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
							case 4060:
							case 40501:
							case 49918:
							case 49919:
							case 49920:
							case 10928:
							case 10929:
							case 10053:
							case 10054:
							case 10060:
							case 40197:
							case 40540:
							case 40613:
							case 40143:
							case 233:
							case 64:
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