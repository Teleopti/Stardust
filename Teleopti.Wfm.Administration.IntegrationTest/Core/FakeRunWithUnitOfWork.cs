using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	public class FakeRunWithUnitOfWork : IRunWithUnitOfWork
	{
		public void WithGlobalScope(IDataSource dataSource, Action action)
		{
			action();
		}

		public void WithBusinessUnitScope(IDataSource dataSource, IBusinessUnit businessUnit, Action action)
		{
			action();
		}
	}
}