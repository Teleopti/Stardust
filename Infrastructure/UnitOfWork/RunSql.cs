using NHibernate;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	class RunSql : IRunSql
	{
		private readonly ISession _session;

		public RunSql(ISession session)
		{
			_session = session;
		}

		public ISqlQuery Create(string sqlCommand)
		{
			return new SqlQuery(_session.CreateSQLQuery(sqlCommand));
		}
	}
}