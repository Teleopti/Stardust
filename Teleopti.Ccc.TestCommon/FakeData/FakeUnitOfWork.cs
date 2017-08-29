using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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
			return false;
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return null;
		}

		public IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			return null;
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
			throw new NotImplementedException();
		}

		public void AfterSuccessfulTx(Action func)
		{
			func();
		}
	}
}