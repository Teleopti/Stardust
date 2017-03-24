using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IPersonInfoMapper
	{
		PersonInfo Map(IPersonInfoModel personInfoModel);
	}
}