using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public string Name { get; private set; }
		public long? NumberOfLiveUnitOfWorks { get; private set; }
		public IAuditSetter AuditSetting { get; private set; }
		public string ConnectionString { get; private set; }
		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			throw new NotImplementedException();
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork();
		}
	}
}