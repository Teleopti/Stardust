using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IConfigReader
	{
		NameValueCollection AppSettings { get; }
		ConnectionStringSettingsCollection ConnectionStrings { get; }
	}
}