using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface ILogOnOff
	{
		void LogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit);
	}
}