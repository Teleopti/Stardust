﻿using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class ReadModelTestAttribute : IoCTestAttribute
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