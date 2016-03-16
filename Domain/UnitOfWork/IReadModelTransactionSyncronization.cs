using System;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public interface IReadModelTransactionSyncronization
	{
		void OnSuccessfulTransaction(Action action);
	}
}