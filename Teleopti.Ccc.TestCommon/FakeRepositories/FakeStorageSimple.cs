using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
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

		public IEnumerable<IRootChangeInfo> Commit()
		{
			return null;
		}
	}
}