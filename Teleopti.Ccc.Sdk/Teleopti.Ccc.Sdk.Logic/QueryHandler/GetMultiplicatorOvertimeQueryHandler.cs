using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetMultiplicatorOvertimeQueryHandler : IHandleQuery<GetMultiplicatorOvertimeQueryDto, ICollection<MultiplicatorDto>>
	{
		private readonly IMultiplicatorRepository _multiplicatorRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetMultiplicatorOvertimeQueryHandler(IMultiplicatorRepository multiplicatorRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_multiplicatorRepository = multiplicatorRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}
		public ICollection<MultiplicatorDto> Handle(GetMultiplicatorOvertimeQueryDto query)
		{
			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				IDisposable filter = null;
				if (query.LoadDeleted)
				{
					filter = unitOfWork.DisableFilter(QueryFilter.Deleted);
				}
				var exports = _multiplicatorRepository.LoadAllByTypeAndSortByName(MultiplicatorType.Overtime);
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
