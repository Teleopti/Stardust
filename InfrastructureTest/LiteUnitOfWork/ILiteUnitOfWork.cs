using NHibernate;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public interface ILiteUnitOfWork
	{
		ISQLQuery CreateSqlQuery(string queryString);
	}
}