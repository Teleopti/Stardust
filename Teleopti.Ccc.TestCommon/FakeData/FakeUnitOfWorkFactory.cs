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

		public string Name { get; set; }
		public string ConnectionString { get; set; }

		public FakeUnitOfWorkFactory(IFakeStorage storage)
		{
			_storage = storage;
		}

		public FakeUnitOfWorkFactory(IFakeStorage storage, IEventPopulatingPublisher publisher, IEnumerable<ITransactionHook> hooks)
		{
			_storage = storage;
			_publisher = publisher;
			_hooks = hooks;
		}

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return new FakeUnitOfWork(_storage, _publisher, _hooks);
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return new FakeUnitOfWork(_storage, _publisher, _hooks);
		}

		public bool HasCurrentUnitOfWork()
		{
			return false;
		}

		public void Dispose()
		{
		}
	}
}