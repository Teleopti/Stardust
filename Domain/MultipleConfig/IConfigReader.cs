using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.MultipleConfig
{
	public interface IConfigReader
	{
		string AppConfig(string name);
		NameValueCollection AppSettings { get; }
		ConnectionStringSettingsCollection ConnectionStrings { get; }
	}

}