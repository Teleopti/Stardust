using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IGroupingReadOnlyRepository
	{
		IEnumerable<ReadOnlyGroupPage> AvailableGroupPages();
		IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate);
		ReadOnlyGroupPage GetGroupPage(Guid groupPageId);
		IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnly queryDate);
		IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate);
	    void UpdateGroupingReadModel(ICollection<Guid> inputIds);
        void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds);
        void UpdateGroupingReadModelData(ICollection<Guid> inputIds);
		IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnlyPeriod queryDateRange);
		IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnlyPeriod queryRange);
		IEnumerable<ReadOnlyGroupDetail> DetailsForGroups(Guid[] groupIds, DateOnlyPeriod queryRange);
		IEnumerable<ReadOnlyGroupDetail> DetailsForPeople(IEnumerable<Guid> peopleIdCollection);
		IEnumerable<ReadOnlyGroupPage> AvailableGroupsBasedOnPeriod(DateOnlyPeriod period);
		IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnlyPeriod period, params Guid[] groupPageIds);
		IEnumerable<ReadOnlyGroupDetail> FindGroups(IEnumerable<Guid> groupIds, DateOnlyPeriod period);
		IEnumerable<ReadOnlyGroup> AllAvailableGroups(DateOnlyPeriod period);
	}

	public class ReadOnlyGroupDetail : IPersonAuthorization
	{
		public Guid PageId { get; set; }
		public string GroupName { get; set; }
		public Guid GroupId { get; set; }
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public class ReadOnlyGroupPage
	{
		public string PageName { get; set; }
		public Guid PageId { get; set; }
	}

	public class ReadOnlyGroup: IPersonAuthorization
	{
		public Guid PageId { get; set; }
		public string PageName { get; set; }
		public string GroupName { get; set; }
		public Guid GroupId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid PersonId { get; set; }
	}
}