using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
		[IsNotDeadCode("Derived classes aren't instantiated by code but auto wired by ioc. Unfortunatly we won't see if concrete types aren't used....")]
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