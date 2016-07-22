using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class WithUnitOfWorkWithRecurringEvents
	{
		private readonly WithUnitOfWork _unitOfWork;
		private readonly IEventPublisher _publisher;

		public WithUnitOfWorkWithRecurringEvents(WithUnitOfWork unitOfWork, IEventPublisher publisher)
		{
			_unitOfWork = unitOfWork;
			_publisher = publisher;
		}

		public void Do(Action action)
		{
			_unitOfWork.Do(action);
			_publisher.Publish(new TenantMinuteTickEvent());
		}
	}
}