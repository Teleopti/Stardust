using System;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class AuthenticationResultAssembler : Assembler<AuthenticationResult,AuthenticationResultDto>
    {
        public override AuthenticationResultDto DomainEntityToDto(AuthenticationResult entity)
        {
            AuthenticationResultDto authenticationResultDto = new AuthenticationResultDto
                                                                  {
                                                                      HasMessage = entity.HasMessage,
                                                                      Message = entity.Message,
                                                                      Successful = entity.Successful
                                                                  };
            return authenticationResultDto;
        }

        public override AuthenticationResult DtoToDomainEntity(AuthenticationResultDto dto)
        {
            throw new NotImplementedException();
        }
    }
}