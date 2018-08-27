using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		private readonly IEventPopulatingPublisher _publisher;
		private readonly IFakeStorage _storage;
		private readonly IEnumerable<ITransactionHook> _hooks;

		public FakeUnitOfWork()
		{
		}

		public FakeUnitOfWork(IFakeStorage storage)
		{
			_storage = storage;
		}

		public FakeUnitOfWork(IFakeStorage storage, IEventPopulatingPublisher publisher, IEnumerable<ITransactionHook> hooks)
		{
			_publisher = publisher;
			_storage = storage;
			_hooks = hooks;
		}

		public void Dispose()
		{
		}

		public IInitiatorIdentifier Initiator()
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			return _storage.Merge(root);
		}

		public bool Contains(IEntity entity)
		{
			throw new NotImplementedException();
		}

		public bool IsDirty()
		{
			return false;
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return PersistAll(null);
		}

		public IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			if (_publisher == null)
				return Enumerable.Empty<IRootChangeInfo>();

			var changes = (_storage as FakeStorage)?.Commit();
			if (changes.IsNullOrEmpty())
				return Enumerable.Empty<IRootChangeInfo>();

			//Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} OnAfterInvocation {changes.Count()}");
			//changes.ForEach(x => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {x.Root.GetType()} {x.Status}"));

			// required for rta tests, really, PA is missing events...
			new ScheduleChangedEventPublisher(_publisher).AfterCompletion(changes);
			_hooks.ForEach(x => x.AfterCompletion(changes));
			_publisher.Publish(new TenantMinuteTickEvent());

			return changes;
		}

		public void Reassociate(IAggregateRoot root)
		{
		}

		public void Reassociate<T>(params IEnumerable<T>[] rootCollectionsCollection) where T : IAggregateRoot
		{
		}

		public void Refresh(IAggregateRoot root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IAggregateRoot root)
		{
		}

		public IDisposable DisableFilter(IQueryFilter filter)
		{
			return new GenericDisposable(() => { });
		}

		public void Flush()
		{
		}

		public void AfterSuccessfulTx(Action action)
		{
			action();
		}
	}
}