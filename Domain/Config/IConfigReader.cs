using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.Config
{
	public interface IConfigReader
	{
		string AppConfig(string name);

		NameValueCollection AppSettings_DontUse { get; }
		ConnectionStringSettingsCollection ConnectionStrings_DontUse { get; }
	}

}