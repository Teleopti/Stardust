using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategory
{
	public class ShiftCategorySelectionModelUpdater : IHandleEvent<TenantDayTickEvent>, IHandleEvent<ShiftCategoryDeletedEvent>, IRunOnHangfire
	{
		public void Handle(TenantDayTickEvent @event)
		{
			throw new System.NotImplementedException();
		}

		public void Handle(ShiftCategoryDeletedEvent @event)
		{
			throw new System.NotImplementedException();
		}
	}
}