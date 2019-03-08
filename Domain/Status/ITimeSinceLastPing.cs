using System;

namespace Teleopti.Ccc.Domain.Status
{
	public interface ITimeSinceLastPing
	{
		TimeSpan Execute(CustomStatusStep step);
	}
}