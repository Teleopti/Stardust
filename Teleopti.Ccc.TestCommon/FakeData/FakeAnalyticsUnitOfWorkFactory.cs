using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeAnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		public string ConnectionString { get; set; }

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork();
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