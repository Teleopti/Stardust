using System;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class AuthenticationResultAssembler : Assembler<AuthenticationQuerierResult, AuthenticationResultDto>
    {
			public override AuthenticationResultDto DomainEntityToDto(AuthenticationQuerierResult entity)
        {
            return new AuthenticationResultDto
                                                                  {
                                                                      HasMessage = !entity.Success,
                                                                      Message = entity.FailReason,
                                                                      Successful = entity.Success
                                                                  };
        }

			public override AuthenticationQuerierResult DtoToDomainEntity(AuthenticationResultDto dto)
        {
            throw new NotImplementedException();
        }
    }
}