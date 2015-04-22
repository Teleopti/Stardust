using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.InfrastructureTest.Rta;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			Resolve<IReadModelUnitOfWorkAspect>().OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			Resolve<IReadModelUnitOfWorkAspect>().OnAfterInvocation(null);
			SetupFixtureForAssembly.RestoreCcc7Database();
		}
	}
}