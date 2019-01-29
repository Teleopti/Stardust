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
	public class GetGroupsForGroupPageForDateRangeQueryHandler : IHandleQuery<GetGroupsForGroupPageForDateRangeQueryDto, ICollection<GroupPageGroupDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetGroupsForGroupPageForDateRangeQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<GroupPageGroupDto> Handle(GetGroupsForGroupPageForDateRangeQueryDto query)
		{
			var queryRange = query.QueryRange.ToDateOnlyPeriod();
			var daysInRange = queryRange.DayCollection();
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var details = _groupingReadOnlyRepository.AvailableGroups(new ReadOnlyGroupPage { PageId = query.PageId }, queryRange);

				var detailsByGroup = details.GroupBy(g => new {g.GroupId, g.GroupName});
				return detailsByGroup.Where(
					p =>
						p.Any(
							d =>
							{
								return daysInRange.Any(date => PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
									date, d));
							})).Select(
										p =>
											new GroupPageGroupDto
											{ GroupName = p.Key.GroupName, Id = p.Key.GroupId }).
					ToList();
			}
		}
	}
}