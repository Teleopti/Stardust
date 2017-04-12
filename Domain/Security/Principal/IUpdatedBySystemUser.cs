using System;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IUpdatedBySystemUser
	{
		IDisposable Context();
	}
}