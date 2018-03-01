using System;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DeadLockVictimThrower
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public DeadLockVictimThrower(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void SetDeadLockPriority(DeadLockVictim deadLockVictim)
		{
			var sql = "SET DEADLOCK_PRIORITY " + (deadLockVictim == DeadLockVictim.Yes ? "LOW" : "HIGH");
			_unitOfWork.Current().Session().CreateSQLQuery(sql).ExecuteUpdate();
		}

		public T ThrowOnDeadlock<T>(Func<T> action)
		{
			try
			{
				return action.Invoke();
			}
			catch (DataSourceException e) when (e.ContainsSqlDeadlock())
			{
				throw new DeadLockVictimException("Transaction deadlocked", e);
			}
		}
	}
}