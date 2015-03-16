using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class LiteTransactionSyncronization : ILiteTransactionSyncronization
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public LiteTransactionSyncronization(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void OnSuccessfulTransaction(Action action)
		{
			((LiteUnitOfWork) _unitOfWork.Current()).OnSuccessfulTransaction(action);
		}
	}
}