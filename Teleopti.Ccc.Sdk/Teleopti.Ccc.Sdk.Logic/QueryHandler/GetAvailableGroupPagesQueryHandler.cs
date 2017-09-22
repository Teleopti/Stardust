using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetAvailableCustomGroupPagesQueryHandler : IHandleQuery<GetAvailableCustomGroupPagesQueryDto, ICollection<GroupPageDto>>
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetAvailableCustomGroupPagesQueryHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<GroupPageDto> Handle(GetAvailableCustomGroupPagesQueryDto query)
		{
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				return
					_groupingReadOnlyRepository.AvailableGroupPages().Select(
						p => new GroupPageDto {PageName = p.PageName, Id = p.PageId}).ToList();
			}
		}
	}
}