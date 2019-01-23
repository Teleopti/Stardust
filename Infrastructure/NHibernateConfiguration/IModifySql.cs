using NHibernate.SqlCommand;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public interface IModifySql
	{
		SqlString Execute(SqlString sqlString);
	}
}