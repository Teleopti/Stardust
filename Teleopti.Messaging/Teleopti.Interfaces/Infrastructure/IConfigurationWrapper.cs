using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IConfigurationWrapper
	{
		IDictionary<string, string> AppSettings { get; }
	}
}