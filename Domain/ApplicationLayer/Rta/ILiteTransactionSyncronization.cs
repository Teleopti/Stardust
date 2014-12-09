using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ILiteTransactionSyncronization
	{
		void OnSuccessfulTransaction(Action action);
	}
}