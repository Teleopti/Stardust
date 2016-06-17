using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	[EnabledBy(Toggles.Intraday_ResourceCalculateReadModel_39200)]
	public class ResourceCalculateReadModelUpdater : IHandleEvent<UpdateResourceCalculateReadModelEvent>, IRunOnStardust
	{
		private readonly CalculateForReadModel _calculateForReadModel;

		public ResourceCalculateReadModelUpdater(CalculateForReadModel calculateForReadModel)
		{
			_calculateForReadModel = calculateForReadModel;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateResourceCalculateReadModelEvent @event)
		{
			var period = new DateOnlyPeriod( new DateOnly( @event.StartDateTime),new DateOnly(@event.EndDateTime));
			_calculateForReadModel.ResourceCalculatePeriod(period);
		}
	}

	public class UpdateResourceCalculateReadModelEvent : EventWithInfrastructureContext
	{
		public DateTime EndDateTime { get; set; }
		public DateTime StartDateTime { get; set; }
	}
}
