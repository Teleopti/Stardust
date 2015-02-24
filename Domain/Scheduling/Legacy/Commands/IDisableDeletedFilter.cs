using System;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IDisableDeletedFilter
	{
		IDisposable Disable();
	}
}
