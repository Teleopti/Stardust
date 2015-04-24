using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IPersonInfoMapper
	{
		PersonInfo Map(PersonInfoModel personInfoModel);
	}
}