using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IStateReader
	{
		TimeZoneInfo UserTimeZone { get; }
		IApplicationData ApplicationScopeData { get; }
	}
}