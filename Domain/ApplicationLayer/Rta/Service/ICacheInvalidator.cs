using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface ICacheInvalidator
	{
		void InvalidateAll();
		void Invalidate();
		void InvalidateSchedules(Guid personId);
	}
}