using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest
{
	public class DomainTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: move this to common
			system.AddModule(new SchedulingCommonModule(configuration));
			system.AddModule(new RuleSetModule(configuration, true));
			//
		}
	}
}