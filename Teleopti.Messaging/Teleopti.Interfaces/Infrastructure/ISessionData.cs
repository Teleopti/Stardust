using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// Data for logged on Person
	/// </summary>
	public interface ISessionData
	{
		TimeZoneInfo TimeZone { get; set; }

		bool MickeMode { get; set; }

		AuthenticationTypeOption AuthenticationTypeOption { get; set; }
	}
}