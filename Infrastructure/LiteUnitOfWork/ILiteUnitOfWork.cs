using NHibernate;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public interface ILiteUnitOfWork
	{
		ISQLQuery CreateSqlQuery(string queryString);
		void Persist(object obj);
		IQuery NamedQuery(string queryName);
	}
}