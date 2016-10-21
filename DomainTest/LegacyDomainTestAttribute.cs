using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest
{
	public class LegacyDomainTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			LegacyTestAttribute.BeforeTest();
			base.Setup(system, configuration);
		}

		protected override void AfterTest()
		{
			LegacyTestAttribute.AfterTest();
		}
	}
}