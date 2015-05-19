using System;

namespace Teleopti.Ccc.Domain.Aop
{
	public interface IReadModelTransactionSyncronization
	{
		void OnSuccessfulTransaction(Action action);
	}
}