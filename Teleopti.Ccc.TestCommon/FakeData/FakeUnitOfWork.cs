using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		public void Dispose()
		{
			
		}

		public IInitiatorIdentifier Initiator()
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			return root;
		}

		public bool Contains(IEntity entity)
		{
			throw new NotImplementedException();
		}

		public bool IsDirty()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return null;
		}

		public IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public int? DatabaseVersion(IAggregateRoot root, bool usePessimisticLock = false)
		{
			throw new NotImplementedException();
		}

		public IDisposable DisableFilter(IQueryFilter filter)
		{
			return new GenericDisposable(() =>{});
		}

		public void Flush()
		{
			throw new NotImplementedException();
		}

		public void AfterSuccessfulTx(Action func)
		{
			throw new NotImplementedException();
		}
	}
}