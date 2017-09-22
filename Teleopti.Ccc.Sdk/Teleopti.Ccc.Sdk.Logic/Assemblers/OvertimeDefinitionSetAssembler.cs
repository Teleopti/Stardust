using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class OvertimeDefinitionSetAssembler : Assembler<IMultiplicatorDefinitionSet,OvertimeDefinitionSetDto>
    {
        private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

        public OvertimeDefinitionSetAssembler(IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
        {
            _multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
        }

        public override OvertimeDefinitionSetDto DomainEntityToDto(IMultiplicatorDefinitionSet entity)
        {
            return new OvertimeDefinitionSetDto
                       {
                           Description = entity.Name,
                           Id = entity.Id.GetValueOrDefault(Guid.Empty),
                           IsDeleted = ((IDeleteTag)entity).IsDeleted
                       };
        }

        public override IMultiplicatorDefinitionSet DtoToDomainEntity(OvertimeDefinitionSetDto dto)
        {
            return _multiplicatorDefinitionSetRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}