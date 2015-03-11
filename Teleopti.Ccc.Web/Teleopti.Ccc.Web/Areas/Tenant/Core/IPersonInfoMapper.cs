using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPersonInfoMapper
	{
		PersonInfo Map(PersonInfoModel personInfoModel);
	}
}