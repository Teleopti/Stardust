using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		public string Name { get; set; }
		public long? NumberOfLiveUnitOfWorks { get; set; }
		public IAuditSetter AuditSetting { get; set; }
		public string ConnectionString { get; set; }

		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			return new FakeUnitOfWork();
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

		private bool _hasCurrentUnitOfWork;
		public bool HasCurrentUnitOfWork()
		{
			return _hasCurrentUnitOfWork;
		}

		public void SetHasCurrentUnitOfWork()
		{
			_hasCurrentUnitOfWork = true;
		}

		public void Dispose()
		{
		}

	}
}