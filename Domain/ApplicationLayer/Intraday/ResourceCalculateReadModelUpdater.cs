using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	[EnabledBy(Toggles.Intraday_ResourceCalculateReadModel_39200)]
	public class ResourceCalculateReadModelUpdater : IHandleEvent<UpdateResourceCalculateReadModelEvent>, IRunOnStardust
	{
		private readonly CalculateForReadModel _calculateForReadModel;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;

		public ResourceCalculateReadModelUpdater(CalculateForReadModel calculateForReadModel, ICurrentUnitOfWorkFactory current)
		{
			_calculateForReadModel = calculateForReadModel;
			_currentFactory = current;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateResourceCalculateReadModelEvent @event)
		{
			var period = new DateOnlyPeriod(new DateOnly(@event.StartDateTime), new DateOnly(@event.EndDateTime));
			_calculateForReadModel.ResourceCalculatePeriod(period);
			var current = _currentFactory.Current().CurrentUnitOfWork();
			//an ugly solution for bug 39594
			if (current.IsDirty())
				current.Clear();
		}
	}

	public class UpdateResourceCalculateReadModelEvent : EventWithInfrastructureContext
	{
		public DateTime EndDateTime { get; set; }
		public DateTime StartDateTime { get; set; }
	}
}
