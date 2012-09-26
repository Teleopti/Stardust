using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler : IHandleQuery<GetMultiplicatorDefinitionSetShiftAllowanceDto, ICollection<DefinitionSetDto>>
	{
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ICollection<DefinitionSetDto> Handle(GetMultiplicatorDefinitionSetShiftAllowanceDto query)
		{
			
		}
	}
}
