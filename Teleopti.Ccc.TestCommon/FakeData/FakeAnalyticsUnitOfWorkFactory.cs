using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeAnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		public string ConnectionString { get; set; }

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork(new FakeStorage());
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork(new FakeStorage());
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}