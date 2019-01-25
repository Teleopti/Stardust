using System.Text.RegularExpressions;
using NHibernate.SqlCommand;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AddForceSeekOnAssignmentLayers : IModifySql
	{
		public SqlString Execute(SqlString sqlString)
		{
			const string pattern = @"left outer join dbo.ShiftLayer\s+(\S+)\s+";
			var match = new Regex(pattern).Match(sqlString.ToString());
			return sqlString.Replace(match.Value, match.Value + "WITH (FORCESEEK) ");
		}
	}
}