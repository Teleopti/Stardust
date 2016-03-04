using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface ISetup
	{
		void Setup(ISystem system, IIocConfiguration configuration);
	}
}