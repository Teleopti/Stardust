using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IPersonInfoHelper
	{
		PersonInfo Create(IPersonInfoModel personInfoModel);
		Tenant GetCurrentTenant();
	}
}