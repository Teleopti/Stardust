using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	[EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754, Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly UpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;

		public UpdateStaffingLevelReadModelHandler(UpdateStaffingLevelReadModel updateStaffingLevelReadModel, ICurrentUnitOfWorkFactory current)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_currentFactory = current;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			var period = new DateTimePeriod(@event.StartDateTime, @event.EndDateTime);
			_updateStaffingLevelReadModel.Update(period);
			var current = _currentFactory.Current().CurrentUnitOfWork();
			//an ugly solution for bug 39594
			if (current.IsDirty())
				current.Clear();
		}
	}
}
