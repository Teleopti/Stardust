using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.Infrastructure.Service
{
	public class DeadLockVictimPriority
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public DeadLockVictimPriority(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Specify(DeadLockVictim deadLockVictim)
		{
			var sql = "SET DEADLOCK_PRIORITY " + (deadLockVictim == DeadLockVictim.Yes ? "LOW" : "HIGH");
			_unitOfWork.Current().Session().CreateSQLQuery(sql).ExecuteUpdate();
		}
	}
}