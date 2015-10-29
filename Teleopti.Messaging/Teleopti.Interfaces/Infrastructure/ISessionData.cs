using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface ISessionData
	{
		TimeZoneInfo TimeZone { get; set; }
	}
}