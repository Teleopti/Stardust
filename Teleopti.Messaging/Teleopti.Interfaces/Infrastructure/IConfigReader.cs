using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Interfaces.Infrastructure
{
	//don't use this one - put it in IocArgs instead and (that uses IAppConfigReader)
	public interface IConfigReader
	{
		NameValueCollection AppSettings { get; }
		ConnectionStringSettingsCollection ConnectionStrings { get; }
	}
}