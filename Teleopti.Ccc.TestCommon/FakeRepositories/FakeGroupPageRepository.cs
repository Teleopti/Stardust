using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeGroupPageRepository : IGroupPageRepository
	{
		private readonly IList<IGroupPage> _groupPages = new List<IGroupPage>();

		public void Add(IGroupPage root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IGroupPage root)
		{
			throw new NotImplementedException();
		}

		public IGroupPage Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IGroupPage Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IGroupPage> LoadAll()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public IList<IGroupPage> GetGroupPagesForPerson(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void Has(IGroupPage groupPage)
		{
			_groupPages.Add(groupPage);
		}
	}
}