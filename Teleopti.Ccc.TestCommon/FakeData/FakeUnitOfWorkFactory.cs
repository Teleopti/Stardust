using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly FakeStorage _storage;

		public string Name { get; set; }
		public IAuditSetter AuditSetting { get; set; }
		public string ConnectionString { get; set; }

		public FakeUnitOfWorkFactory(FakeStorage storage)
		{
			_storage = storage;
		}

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork(_storage);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return new FakeUnitOfWork(_storage);
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork(_storage);
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