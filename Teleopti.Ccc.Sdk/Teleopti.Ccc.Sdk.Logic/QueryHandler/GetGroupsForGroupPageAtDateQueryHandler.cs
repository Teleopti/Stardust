using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetGroupsForGroupPageAtDateQueryHandler : IHandleQuery<GetGroupsForGroupPageAtDateQueryDto, ICollection<GroupPageGroupDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GetGroupsForGroupPageAtDateQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<GroupPageGroupDto> Handle(GetGroupsForGroupPageAtDateQueryDto query)
		{
			var queryDate = query.QueryDate.ToDateOnly();
			var details = _groupingReadOnlyRepository.AvailableGroups(new ReadOnlyGroupPage{PageId = query.PageId},queryDate);
				
			var detailsByGroup = from d in details
			                     group d by new {d.GroupId,d.GroupName}
			                     into g
			                     select g;

			return detailsByGroup.Where(
				p =>
				p.Any(
					d =>
					PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
					                                                             queryDate, d))).Select(
					                                                             	p =>
					                                                             	new GroupPageGroupDto
					                                                             		{GroupName = p.Key.GroupName, Id = p.Key.GroupId}).
				ToList();
		}
	}
}