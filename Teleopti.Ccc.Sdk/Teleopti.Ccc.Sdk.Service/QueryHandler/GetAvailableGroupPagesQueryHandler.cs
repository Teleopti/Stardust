using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
	public class GetAvailableCustomGroupPagesQueryHandler : IHandleQuery<GetAvailableCustomGroupPagesQueryDto, ICollection<GroupPageDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GetAvailableCustomGroupPagesQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public ICollection<GroupPageDto> Handle(GetAvailableCustomGroupPagesQueryDto query)
		{
			return
				_groupingReadOnlyRepository.AvailableGroupPages().Select(
					p => new GroupPageDto {PageName = p.PageName, Id = p.PageId}).ToList();
		}
	}
}