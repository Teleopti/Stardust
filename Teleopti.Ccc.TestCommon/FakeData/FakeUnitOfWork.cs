using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		private readonly FakeStorage _storage;

		public FakeUnitOfWork(FakeStorage storage)
		{
			_storage = storage;
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
			var item = _storage.Get<T>(root.Id.GetValueOrDefault());
			_storage.Remove(item);
			_storage.Add(root);
			return root;
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
			yield break;
		}

		public IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			yield break;
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
			return new GenericDisposable(() =>{});
		}

		public void Flush()
		{
		}

		public void AfterSuccessfulTx(Action func)
		{
			func();
		}
	}
}