using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IAssembler<TDo, TDto>
    {
        IEnumerable<TDto> DomainEntitiesToDtos(IEnumerable<TDo> entityCollection);
        IEnumerable<TDo> DtosToDomainEntities(IEnumerable<TDto> dtoCollection);
        TDto DomainEntityToDto(TDo entity);
        TDo DtoToDomainEntity(TDto dto);
    }
}