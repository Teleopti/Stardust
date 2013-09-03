using System;
using NHibernate;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SqlQuery : ISqlQuery
	{
		private readonly IQuery _sqlQuery;

		public SqlQuery(IQuery sqlQuery)
		{
			_sqlQuery = sqlQuery;
		}

		public ISqlQuery SetString(string parameter, string value)
		{
			_sqlQuery.SetString(parameter, value);
			return this;
		}

		public ISqlQuery SetDateTime(string parameter, DateTime value)
		{
			_sqlQuery.SetDateTime(parameter, value);
			return this;
		}

		public ISqlQuery SetGuid(string parameter, Guid value)
		{
			_sqlQuery.SetGuid(parameter, value);
			return this;
		}

		public void Execute()
		{
			_sqlQuery.ExecuteUpdate();
		}
	}
}