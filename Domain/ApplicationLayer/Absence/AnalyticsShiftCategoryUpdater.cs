using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Absence
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayAbsence_38301)]
	public class AnalyticsShiftCategoryUpdater :
		IHandleEvent<ShiftCategoryChangedEvent>,
		IHandleEvent<ShiftCategoryDeletedEvent>,
		IRunOnHangfire
	{
		public void Handle(ShiftCategoryChangedEvent @event)
		{
			//throw new NotImplementedException();
		}

		public void Handle(ShiftCategoryDeletedEvent @event)
		{
			//throw new NotImplementedException();
		}
	}
}