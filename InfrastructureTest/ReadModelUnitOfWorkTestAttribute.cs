using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.InfrastructureTest.Rta;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			Resolve<IReadModelUnitOfWorkAspect>().OnBeforeInvocation();
		}

		protected override void AfterTest()
		{
			Resolve<IReadModelUnitOfWorkAspect>().OnAfterInvocation(null);
		}
	}
}