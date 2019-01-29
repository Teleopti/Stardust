using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetGroupsForGroupPageAtDateQueryHandler : IHandleQuery<GetGroupsForGroupPageAtDateQueryDto, ICollection<GroupPageGroupDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetGroupsForGroupPageAtDateQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}
		
		public ICollection<GroupPageGroupDto> Handle(GetGroupsForGroupPageAtDateQueryDto query)
		{
			var queryDate = query.QueryDate.ToDateOnly();
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var details = _groupingReadOnlyRepository.AvailableGroups(new ReadOnlyGroupPage {PageId = query.PageId}, queryDate);

				var detailsByGroup = details.GroupBy(g => new {g.GroupId, g.GroupName});
				return detailsByGroup.Where(
					p =>
						p.Any(
							d =>
								PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
									queryDate, d))).Select(
										p =>
											new GroupPageGroupDto
											{GroupName = p.Key.GroupName, Id = p.Key.GroupId}).
					ToList();
			}
		}
	}
}