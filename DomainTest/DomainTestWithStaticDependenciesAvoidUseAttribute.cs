using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest
{
	/// <summary>
	/// If possible, use [DomainTest] instead (or fix so that could be used by removing static dependencies in code)
	/// </summary>
	public class DomainTestWithStaticDependenciesAvoidUseAttribute : DomainTestAttribute
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