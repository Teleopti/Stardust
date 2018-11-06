﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class AnalyticsUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon();
			base.BeforeTest();
			Resolve<IEnumerable<IAspect>>()
				.OfType<IAnalyticsUnitOfWorkAspect>()
				.Single()
				.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			Resolve<IEnumerable<IAspect>>()
				.OfType<IAnalyticsUnitOfWorkAspect>()
				.Single()
				.OnAfterInvocation(null, null);
			InfrastructureTestStuff.AfterWithLogon();
		}
	}
}