using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IPersonInfoMapper
	{
		PersonInfo Map(IPersonInfoModel personInfoModel);
	}
}