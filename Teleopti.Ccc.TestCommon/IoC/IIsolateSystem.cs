using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIsolateSystem
	{
		void Isolate(IIsolate isolate);
	}
	
	public interface IExtendSystem
	{
		void Extend(IExtend extend, IocConfiguration configuration);
	}

}