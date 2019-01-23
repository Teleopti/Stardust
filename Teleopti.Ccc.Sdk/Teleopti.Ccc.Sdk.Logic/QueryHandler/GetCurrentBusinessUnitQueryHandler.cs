using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetCurrentBusinessUnitQueryHandler : IHandleQuery<GetCurrentBusinessUnitQueryDto, ICollection<BusinessUnitDto>>
    {
	    private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

	    public GetCurrentBusinessUnitQueryHandler(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
	    {
		    _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
	    }

	    public ICollection<BusinessUnitDto> Handle(GetCurrentBusinessUnitQueryDto query)
        {
			var identity = ((ITeleoptiIdentity)_currentTeleoptiPrincipal.Current().Identity);
            return new[] {new BusinessUnitDto{Id = identity.BusinessUnitId,Name = identity.BusinessUnitName}};
        }
    }
}
