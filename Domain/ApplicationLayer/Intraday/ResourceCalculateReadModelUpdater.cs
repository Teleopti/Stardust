using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
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
			//get the scheduled period or inside the method below
			var model =
				_calculateForReadModel.ResourceCalculatePeriod(new DateOnlyPeriod(new DateOnly(2016,06,15),
					new DateOnly(2016, 06, 15).AddDays(1)));
			//save model to DB
		}
	}

	public class UpdateResourceCalculateReadModelEvent : EventWithInfrastructureContext
	{
		
	}
}
