using System.Collections.Specialized;

namespace Teleopti.Ccc.Domain.Config
{
	public interface IConfigReader
	{
		string AppConfig(string name);
		string ConnectionString(string name);

		NameValueCollection AppSettings_DontUse { get; }
	}

}