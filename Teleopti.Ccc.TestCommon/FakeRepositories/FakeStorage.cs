using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public interface IFakeStorage
	{
		void Add(IAggregateRoot entity);
		void Remove(IAggregateRoot entity);
		T Get<T>(Guid id) where T : IAggregateRoot;
		IEnumerable<T> LoadAll<T>();
		T Merge<T>(T root) where T : class, IAggregateRoot;
	}

	public class FakeStorageSimple : IFakeStorage
	{
		private readonly List<IAggregateRoot> _data = new List<IAggregateRoot>();

		public void Add(IAggregateRoot entity)
		{
			_data.Add(entity);
		}

		public void Remove(IAggregateRoot entity)
		{
			_data.Remove(entity);
		}

		public T Get<T>(Guid id) where T : IAggregateRoot
		{
			return _data.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(id));
		}

		public IEnumerable<T> LoadAll<T>()
		{
			return _data.OfType<T>().ToArray();
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			return root;
		}
	}

	public class FakeStorage : IFakeStorage
	{
		//maybe these needs to be thread safe coll types as well? let's see....
		private readonly List<IAggregateRoot> _legacy = new List<IAggregateRoot>();
		private readonly List<IAggregateRoot> _added = new List<IAggregateRoot>();
		private readonly List<IAggregateRoot> _removed = new List<IAggregateRoot>();
		private readonly List<IAggregateRoot> _modified = new List<IAggregateRoot>();
		private readonly List<IAggregateRoot> _comitted = new List<IAggregateRoot>();

		private static readonly object transactionLock = new object();

		public void Add(IAggregateRoot entity)
		{
			lock (transactionLock)
			{
				_legacy.Add(entity);
				_added.Add(entity);
			}
		}

		public void Remove(IAggregateRoot entity)
		{
			lock (transactionLock)
			{
				_legacy.Remove(entity);
				_removed.Add(entity);	
			}
		}

		public T Get<T>(Guid id) where T : IAggregateRoot
		{
			// mimic a real get would be nice
			// getting transient too, not removed, and throwing if it doesnt exist, right?
			lock (transactionLock)
			{
				return _legacy.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(id));
			}
		}

		public IEnumerable<T> LoadAll<T>()
		{
			// mimic a real load would be nice
			// getting only from db, right?
			lock (transactionLock)
			{
				return _legacy.OfType<T>().ToArray();
			}
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			if (!root.Id.HasValue)
			{
				// some tests merge entities that are transient.
				// sounds like bad tests, but dont want to fix now.
				if (!_legacy.Contains(root))
					_legacy.Add(root);
				_modified.Add(root);
			}
			else
			{
				var item = _legacy.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(root.Id));
				_legacy.Remove(item);
				_legacy.Add(root);
				_modified.Add(root);
			}
			return root;
		}

		public IEnumerable<IRootChangeInfo> Commit()
		{
			RootChangeInfo[] updates;
			RootChangeInfo[] removals;
			RootChangeInfo[] adds;
			lock (transactionLock)
			{
				adds = _added
					.Select(x => new RootChangeInfo(x, DomainUpdateType.Insert))
					.ToArray();
				_comitted.AddRange(_added);
				_added.Clear();

				removals = _removed
					.Select(x => new RootChangeInfo(x, DomainUpdateType.Delete))
					.ToArray();
				_removed.ForEach(x => _comitted.Remove(x));
				_removed.Clear();

				var dirty = _comitted
					.OfType<IAggregateRootWithEvents>()
					.Where(x => x.HasEvents());
				updates = _modified
					.Union(dirty)
					.Select(x => new RootChangeInfo(x, DomainUpdateType.Update))
					.ToArray();
				_modified.Clear();
			}
			
			return adds
				.Concat(removals)
				.Concat(updates)
				.Cast<IRootChangeInfo>()
				.ToArray();
		}
	}
}