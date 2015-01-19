using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class ReadModelTestAttribute : IoCTestAttribute
	{
		protected override void BeforeTest()
		{
			Resolve<IReadModelUnitOfWorkAspect>().OnBeforeInvokation();
		}

		protected override void AfterTest()
		{
			Resolve<IReadModelUnitOfWorkAspect>().OnAfterInvokation(null);
			DataSourceHelper.RestoreCcc7Database(0, () => {});
		}
	}
}