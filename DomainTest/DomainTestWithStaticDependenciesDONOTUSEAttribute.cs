using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest
{
	public class DomainTestWithStaticDependenciesDONOTUSEAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			TestWithStaticDependenciesAvoidUseAttribute.BeforeTest();
			base.Setup(system, configuration);
		}

		protected override void AfterTest()
		{
			TestWithStaticDependenciesAvoidUseAttribute.AfterTest();
		}
	}
}