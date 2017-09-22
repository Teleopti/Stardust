using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetMultiplicatorShiftAllowanceQueryHandler: IHandleQuery<GetMultiplicatorShiftAllowanceQueryDto, ICollection<MultiplicatorDto>>
	{
		private readonly IMultiplicatorRepository _multiplicatorRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetMultiplicatorShiftAllowanceQueryHandler(IMultiplicatorRepository multiplicatorRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_multiplicatorRepository = multiplicatorRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<MultiplicatorDto> Handle(GetMultiplicatorShiftAllowanceQueryDto query)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.LoadDeletedIfSpecified(query.LoadDeleted))
				{
					var exports = _multiplicatorRepository.LoadAllByTypeAndSortByName(MultiplicatorType.OBTime);
					var result = exports.Select(x => new MultiplicatorDto
						{
							Color = new ColorDto(x.DisplayColor),
							Id = x.Id,
							Multiplicator = x.MultiplicatorValue,
							MultiplicatorType = (MultiplicatorTypeDto) x.MultiplicatorType,
							Name = x.Description.Name,
							PayrollCode = x.ExportCode,
							ShortName = x.Description.ShortName,
							IsDeleted = ((IDeleteTag) x).IsDeleted
						}).ToArray();
					return result;
				}
			}
		}
	}
}
