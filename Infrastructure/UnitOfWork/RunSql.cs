using NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class RunSql : IRunSql
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public RunSql(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public ISqlQuery Create(string sqlCommand)
		{
			return new SqlQuery(_unitOfWork.Session().CreateSQLQuery(sqlCommand));
		}
	}
}