using System;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public interface ILiteTransactionSyncronization
	{
		void OnSuccessfulTransaction(Action action);
	}
}