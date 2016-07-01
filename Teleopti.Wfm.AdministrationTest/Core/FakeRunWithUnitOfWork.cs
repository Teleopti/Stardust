using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest.Core
{
	public class FakeRunWithUnitOfWork : IRunWithUnitOfWork
	{
		public void WithGlobalScope(IDataSource dataSource, Action<ICurrentUnitOfWork> action)
		{
			action(null);
		}

		public void WithBusinessUnitScope(IDataSource dataSource, IBusinessUnit businessUnit, Action<ICurrentUnitOfWork> action)
		{
			action(null);
		}
	}
}