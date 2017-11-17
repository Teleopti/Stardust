using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			base.BeforeTest();
			Resolve<IEnumerable<IAspect>>()
				.OfType<IReadModelUnitOfWorkAspect>()
				.Single()
				.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			Resolve<IEnumerable<IAspect>>()
				.OfType<IReadModelUnitOfWorkAspect>()
				.Single()
				.OnAfterInvocation(null, null);
			SetupFixtureForAssembly.RestoreCcc7Database();
		}
	}
}