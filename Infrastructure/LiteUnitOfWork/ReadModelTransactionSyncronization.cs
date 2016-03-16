using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelTransactionSyncronization : IReadModelTransactionSyncronization
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public ReadModelTransactionSyncronization(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void OnSuccessfulTransaction(Action action)
		{
			((LiteUnitOfWork) _unitOfWork.Current()).OnSuccessfulTransaction(action);
		}
	}
}