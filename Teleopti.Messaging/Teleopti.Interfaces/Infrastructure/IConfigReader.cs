using System.Collections.Specialized;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IConfigReader
	{
		NameValueCollection AppSettings { get; }
	}
}