using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetMultiplicatorShiftAllowanceQueryHandler: IHandleQuery<GetMultiplicatorShiftAllowanceQueryDto, ICollection<MultiplicatorDto>>
	{
		private readonly IMultiplicatorRepository _multiplicatorRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public GetMultiplicatorShiftAllowanceQueryHandler(IMultiplicatorRepository multiplicatorRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_multiplicatorRepository = multiplicatorRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<MultiplicatorDto> Handle(GetMultiplicatorShiftAllowanceQueryDto query)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IDisposable filter = null;
				if (query.LoadDeleted)
				{
					filter = unitOfWork.DisableFilter(QueryFilter.Deleted);
				}
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
														IsDeleted = ((IDeleteTag)x).IsDeleted
				                                 	}).ToArray();
				if (filter != null)
				{
					filter.Dispose();
				}
				return result;
			}
		}
	}
}
