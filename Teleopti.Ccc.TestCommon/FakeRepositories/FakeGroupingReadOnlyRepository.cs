using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class UnPromiscuousFakeGroupingReadOnlyRepository : FakeGroupingReadOnlyRepository
	{
		public override IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate)
		{
			return base.DetailsForGroup(groupId, queryDate).Where(x => x.GroupId == groupId);
		}
	}

	public class FakeGroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		private ReadOnlyGroupDetail[] _details;
		private IList<ReadOnlyGroupPage> _readOnlyGroupPages;

		public FakeGroupingReadOnlyRepository(params ReadOnlyGroupDetail[] details)
		{
			_details = details;
		}

		public FakeGroupingReadOnlyRepository(IList<ReadOnlyGroupPage> readOnlyGroupPages,
			IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			_readOnlyGroupPages = readOnlyGroupPages;
			_details = readOnlyGroupDetails.ToArray();
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupPages()
		{
			return _readOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate)
		{
			return _details;
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

		public IEnumerable<ReadOnlyGroupDetail> DetailsForPeople(IEnumerable<Guid> peopleIdCollection)
		{
			return _details;
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupsBasedOnPeriod(DateOnlyPeriod period)
		{
			return _readOnlyGroupPages;
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(List<ReadOnlyGroupPage> groupPages, DateOnlyPeriod period)
		{
			return _details;
		}

		public void Has(IList<ReadOnlyGroupPage> readOnlyGroupPages,
			IList<ReadOnlyGroupDetail> readOnlyGroupDetails)
		{
			_readOnlyGroupPages = readOnlyGroupPages;
			_details = readOnlyGroupDetails.ToArray();
		}
	}

}