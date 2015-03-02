using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetCurrentBusinessUnitQueryHandler : IHandleQuery<GetCurrentBusinessUnitQueryDto, ICollection<BusinessUnitDto>>
    {
        public ICollection<BusinessUnitDto> Handle(GetCurrentBusinessUnitQueryDto query)
        {
            var currentBusinessUnit = ((ITeleoptiIdentity) TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit;
            return new[] {new BusinessUnitDto{Id = currentBusinessUnit.Id,Name = currentBusinessUnit.Name}};
        }
    }
}
