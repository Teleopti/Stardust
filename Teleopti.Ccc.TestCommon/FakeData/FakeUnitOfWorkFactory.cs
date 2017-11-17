using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly IFakeStorage _storage;
		private readonly IEventPopulatingPublisher _publisher;
		private readonly IEnumerable<ITransactionHook> _hooks;
		private readonly INow _now;

		public string Name { get; set; }
		public IAuditSetter AuditSetting { get; set; }
		public string ConnectionString { get; set; }


		public FakeUnitOfWorkFactory(IFakeStorage storage)
		{
			_storage = storage;
		}

		public FakeUnitOfWorkFactory(IFakeStorage storage, IEventPopulatingPublisher publisher, IEnumerable<ITransactionHook> hooks, INow now)
		{
			_storage = storage;
			_publisher = publisher;
			_hooks = hooks;
			_now = now;
		}

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork(_storage, _publisher, _hooks, _now);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return new FakeUnitOfWork(_storage, _publisher, _hooks, _now);
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork(_storage, _publisher, _hooks, _now);
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