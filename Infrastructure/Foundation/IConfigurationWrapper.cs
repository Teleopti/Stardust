using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IConfigurationWrapper
	{
		IDictionary<string, string> AppSettings { get; }
	}
}