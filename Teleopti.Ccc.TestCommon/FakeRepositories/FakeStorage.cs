using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStorage
	{
		private readonly IList<IAggregateRoot> _legacy = new List<IAggregateRoot>();

		private readonly IList<IAggregateRoot> _added = new List<IAggregateRoot>();
		private readonly IList<IAggregateRoot> _removed = new List<IAggregateRoot>();
		private readonly IList<IAggregateRoot> _comitted = new List<IAggregateRoot>();

		public void Add(IAggregateRoot entity)
		{
			_legacy.Add(entity);
			_added.Add(entity);
		}

		public void Remove(IAggregateRoot entity)
		{
			_legacy.Remove(entity);
			_removed.Add(entity);
		}

		public T Get<T>(Guid id) where T : IAggregateRoot
		{
			// mimic a real get would be nice
			// getting transient too, not removed, and throwing if it doesnt exist, right?
			return _legacy.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(id));
		}

		public IEnumerable<T> LoadAll<T>()
		{
			// mimic a real load would be nice
			// getting only from db, right?
			return _legacy.OfType<T>().ToArray();
		}

		public IEnumerable<IRootChangeInfo> Commit()
		{
			var adds = _added
				.Select(x => new RootChangeInfo(x, DomainUpdateType.Insert))
				.ToArray();
			_added.ForEach(_comitted.Add);
			_added.Clear();

			var removals = _removed
				.Select(x => new RootChangeInfo(x, DomainUpdateType.Delete))
				.ToArray();
			_removed.ForEach(x => _comitted.Remove(x));
			_removed.Clear();

			var updates = _comitted
				.OfType<IAggregateRootWithEvents>()
				.Where(x => x.HasEvents())
				.Select(x => new RootChangeInfo(x, DomainUpdateType.Update))
				.ToArray();

			return adds
				.Concat(removals)
				.Concat(updates)
				.Cast<IRootChangeInfo>()
				.ToArray();
		}
	}
}