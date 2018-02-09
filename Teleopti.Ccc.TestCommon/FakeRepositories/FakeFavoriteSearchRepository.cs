using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeFavoriteSearchRepository : IFavoriteSearchRepository, IEnumerable<IFavoriteSearch>
	{
		private IList<IFavoriteSearch> storage = new List<IFavoriteSearch>();

		public IEnumerable<IFavoriteSearch> FindAllForPerson(Guid personId, WfmArea area)
		{
			return storage.Where(f => f.Creator.Id == personId && f.WfmArea == area);
		}

		public IEnumerable<IFavoriteSearch> FindByPersonAndName(Guid personId, string name, WfmArea area)
		{
			return storage.Where(f => f.Creator.Id == personId && f.Name == name && f.WfmArea == area);
		}

		public IEnumerator<IFavoriteSearch> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IFavoriteSearch root)
		{
			storage.Add(root);
		}

		public void Remove(IFavoriteSearch root)
		{
			throw new NotImplementedException();
		}

		public IFavoriteSearch Get(Guid id)
		{
			return storage.FirstOrDefault(s => id == s.Id);
		}

		public IFavoriteSearch Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<IFavoriteSearch> LoadAll()
		{
			return storage;
		}
	}
}