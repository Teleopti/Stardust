using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface ITenantUserSetup
	{
		void Apply(ICurrentTenantSession tenantSession, IPerson user, ILogonName logonName);
	}
}