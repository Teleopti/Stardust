using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class PurgeApplicationData : IExecutableCommand
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public PurgeApplicationData(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Execute()
		{
			const int fiveMinutes = 300;
			var unitOfWork = (NHibernateUnitOfWork)_unitOfWorkFactory.CurrentUnitOfWork();
			unitOfWork.Session.CreateSQLQuery("exec dbo.Purge").SetTimeout(fiveMinutes).ExecuteUpdate();
		}
	}
}