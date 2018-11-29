using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeGroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		private ReadOnlyGroupDetail[] _details;
		private IList<ReadOnlyGroup> _groups;
		private IList<ReadOnlyGroupPage> _readOnlyGroupPages;

		public FakeGroupingReadOnlyRepository(params ReadOnlyGroupDetail[] details)
		{
			_details = details;
			_groups = new List<ReadOnlyGroup>();
		}

		public FakeGroupingReadOnlyRepository(IList<ReadOnlyGroupPage> readOnlyGroupPages,
			IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			_readOnlyGroupPages = readOnlyGroupPages;
			_details = readOnlyGroupDetails.ToArray();
			_groups = new List<ReadOnlyGroup>();
		}
		
		public IEnumerable<ReadOnlyGroupPage> AvailableGroupPages()
		{
			return _readOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate)
		{
			return _details;
		}

		public ReadOnlyGroupPage GetGroupPage(Guid groupPageId)
		{
			return _readOnlyGroupPages.Single(p => p.PageId == groupPageId);
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnly queryDate)
		{
			return _details;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(IEnumerable<ReadOnlyGroupPage> groupPages, DateOnly queryDate)
		{
			return _details;
		}

		public virtual IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate)
		{
			return _details;
		}

		public void UpdateGroupingReadModel(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}

		public void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}

		public void UpdateGroupingReadModelData(ICollection<Guid> inputIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnlyPeriod queryDateRange)
		{
			return _details;
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnlyPeriod queryRange)
		{
			return _details;
		}

		public FakeGroupingReadOnlyRepository Has(ReadOnlyGroupDetail model)
		{
			_details = _details.Concat(new[] { model }).ToArray();
			return this;
		}

		public void Has(ReadOnlyGroup[] groups)
		{
			_groups = _groups.Concat(groups).ToArray();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForPeople(IEnumerable<Guid> peopleIdCollection)
		{
			return _details.Where(d => peopleIdCollection.Contains(d.PersonId));
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupsBasedOnPeriod(DateOnlyPeriod period)
		{
			return _readOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnlyPeriod period, params Guid[] groupPageIds)
		{
			return _details;
		}

		public IEnumerable<ReadOnlyGroupDetail> FindGroups(IEnumerable<Guid> groupIds, DateOnlyPeriod period)
		{
			return _details;
		}

		public void Has(IList<ReadOnlyGroupPage> readOnlyGroupPages,
			IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			_readOnlyGroupPages = readOnlyGroupPages;
			_details = readOnlyGroupDetails.ToArray();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroups(Guid[] groupIds, DateOnlyPeriod queryRange)
		{
			return _details.Where(detail => groupIds.Contains(detail.GroupId));
		}

		public IEnumerable<ReadOnlyGroup> AllAvailableGroups(DateOnlyPeriod period)
		{
			return _groups;
		}
	}
}