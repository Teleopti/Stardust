using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		public void Dispose()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			throw new NotImplementedException();
		}

		public void Reassociate(IAggregateRoot root)
		{
			throw new NotImplementedException();
		}

		public void Reassociate<T>(params IEnumerable<T>[] rootCollectionsCollection) where T : IAggregateRoot
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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