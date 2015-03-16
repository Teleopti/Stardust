using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ILiteTransactionSyncronization
	{
		void OnSuccessfulTransaction(Action action);
	}
}