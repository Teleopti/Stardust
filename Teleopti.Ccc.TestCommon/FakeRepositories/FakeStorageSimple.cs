using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStorageSimple : IFakeStorage
	{
		private readonly List<IAggregateRoot> _data = new List<IAggregateRoot>();
		private readonly object lockObject = new object();
		
		public void Add(IAggregateRoot entity)
		{
			lock (lockObject)
			{
				_data.Add(entity);				
			}
		}

		public void Remove(IAggregateRoot entity)
		{
			lock (lockObject)
			{
				_data.Remove(entity);				
			}
		}
		
		public T Get<T>(Guid id) where T : IAggregateRoot
		{
			lock (lockObject)
			{
				return _data.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(id));
			}
		}

		public IEnumerable<T> LoadAll<T>()
		{
			lock (lockObject)
			{
				return _data.OfType<T>().ToArray();
			}
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			lock (lockObject)
			{
				if (!root.Id.HasValue)
				{
					// some tests merge entities that are transient.
					// sounds like bad tests, but dont want to fix now.
					if (!_data.Contains(root))
						_data.Add(root);
				}
				else
				{
					var item = _data.OfType<T>().ToArray().SingleOrDefault(x => x.Id.Equals(root.Id));
					_data.Remove(item);
					_data.Add(root);
				}

				return root;
			}
		}

		public IEnumerable<IRootChangeInfo> Commit()
		{
			return null;
		}
	}
}