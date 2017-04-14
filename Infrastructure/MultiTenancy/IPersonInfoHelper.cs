using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IPersonInfoHelper
	{
		PersonInfo Create(IPersonInfoModel personInfoModel);
		Tenant GetCurrentTenant();
	}
}