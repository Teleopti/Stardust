using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeGroupPageRepository : IGroupPageRepository
	{
		private readonly IList<IGroupPage> _groupPages = new List<IGroupPage>();

		public void Add(IGroupPage root)
		{
			_groupPages.Add(root);
		}

		public void Remove(IGroupPage root)
		{
			_groupPages.Remove(root);
		}

		public IGroupPage Get(Guid id)
		{
			return _groupPages.FirstOrDefault(x => x.Id == id);
		}

		public IGroupPage Load(Guid id)
		{
			return _groupPages.First(x => x.Id == id);
		}

		public IList<IGroupPage> LoadAll()
		{
			return _groupPages;
		}

		public IList<IGroupPage> LoadAllGroupPageBySortedByDescription()
		{
			throw new NotImplementedException();
		}

		public IList<IGroupPage> LoadAllGroupPageWhenPersonCollectionReAssociated()
		{
			return _groupPages;
		}

		public IList<IGroupPage> LoadGroupPagesByIds(IEnumerable<Guid> groupPageIds)
		{
			return _groupPages.Where(x => groupPageIds.ToList().Contains(x.Id.Value)).ToList();
		}

		public IList<IGroupPage> GetGroupPagesForPerson(Guid personId)
		{
			return _groupPages.Where(x => x.RootGroupCollection.Any(y => y.PersonCollection.Any(p => p.Id == personId))).ToList();
		}

		public void Has(IGroupPage groupPage)
		{
			_groupPages.Add(groupPage);
		}
	}
}