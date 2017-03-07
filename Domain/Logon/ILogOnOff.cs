using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface ILogOnOff
	{
		void LogOn(string tenant, IPerson user, Guid businessUnitId);
		void LogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit);
		void LogOnWithoutPermissions(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit);
	}
}