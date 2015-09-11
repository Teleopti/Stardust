using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface ICacheInvalidator
	{
		void InvalidateAllForCurrentTenant();
		void InvalidateStateForCurrentTenant();
		void InvalidateSchedulesForCurrentTenant(Guid personId);
		void InvalidateAllForAllTenants();
	}
}