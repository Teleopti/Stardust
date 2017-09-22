using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class UnitOfWorkTransactionEventSyncronization : IEventSyncronization
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public UnitOfWorkTransactionEventSyncronization(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void WhenDone(Action done)
		{
			_unitOfWork.Current().AfterSuccessfulTx(done);
		}
	}
}