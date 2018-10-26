using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon();
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
			InfrastructureTestStuff.AfterWithLogon();
		}
	}
}