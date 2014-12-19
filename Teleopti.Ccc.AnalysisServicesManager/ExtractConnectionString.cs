using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AnalysisServicesManager
{
	public static class ExtractConnectionString
	{
		public static string sqlConnectionStringSet(CommandLineArgument argument)
		{
			string sqlConnectionString;

			if (argument.UseIntegratedSecurity)
			{
				sqlConnectionString =
				string.Format(CultureInfo.InvariantCulture,
							  "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog={1}",
							  argument.SqlServer, argument.SqlDatabase);
			}
			else
			{
				sqlConnectionString =
					string.Format(CultureInfo.InvariantCulture,
								  "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;User ID={1};Password={2};Initial Catalog={3}",
								  argument.SqlServer, argument.SqlUser, argument.SqlPassword, argument.SqlDatabase);
			}
			return sqlConnectionString;
		} 
	}
}
