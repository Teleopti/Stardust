using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface ITenantUserSetup
	{
		void Apply(ICurrentTenantSession tenantSession, IPerson user, ILogonName logonName);
	}
}