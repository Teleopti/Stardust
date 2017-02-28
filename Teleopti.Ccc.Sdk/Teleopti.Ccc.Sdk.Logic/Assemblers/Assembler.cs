using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public abstract class Assembler<TDo, TDto> : IAssembler<TDo, TDto>
    {
        public IEnumerable<TDo> DtosToDomainEntities(IEnumerable<TDto> dtoCollection)
        {
            foreach (TDto dto in dtoCollection)
            {
                yield return DtoToDomainEntity(dto);
            }
        }

        public virtual IEnumerable<TDto> DomainEntitiesToDtos(IEnumerable<TDo> entityCollection)
        {
            foreach (TDo domainEntity in entityCollection)
            {
                yield return DomainEntityToDto(domainEntity);
            }
        }

        public abstract TDto DomainEntityToDto(TDo entity);
        public abstract TDo DtoToDomainEntity(TDto dto);
    }
}