using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICacheInvalidator
	{
		void Invalidate();
		void InvalidateSchedules(Guid personId);
	}
}