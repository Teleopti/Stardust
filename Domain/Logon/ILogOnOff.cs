using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface ILogOnOff
	{
		void ProperLogOn(string tenant, IPerson user, Guid businessUnitId);
		void ProperLogOn(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit);
		void LogOnWithoutClaims(IDataSource dataSource, IPerson user, IBusinessUnit businessUnit);
		void SetupClaims(string tenant);
	}
}