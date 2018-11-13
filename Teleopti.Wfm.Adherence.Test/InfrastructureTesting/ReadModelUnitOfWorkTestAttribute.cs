using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			InfrastructureTestSetup.Before();
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
			InfrastructureTestSetup.After();
		}
	}
}