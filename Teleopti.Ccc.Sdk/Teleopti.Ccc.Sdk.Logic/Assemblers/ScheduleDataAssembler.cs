using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public abstract class ScheduleDataAssembler<TDo, TDto> : Assembler<TDo, TDto>,
                                                             IScheduleDataAssembler<TDo, TDto>
    {
        public IPerson Person { get; set; }
        public IScenario DefaultScenario { get; set; }
        public DateOnly PartDate { get; set; }

        public sealed override TDo DtoToDomainEntity(TDto dto)
        {
            EnsureInjectionForDtoToDo();
            return DtoToDomainEntityAfterValidation(dto);
        }

        protected abstract TDo DtoToDomainEntityAfterValidation(TDto dto);

        protected virtual void EnsureInjectionForDtoToDo()
        {
            if (Person == null)
                throw new InvalidOperationException("You need to set Person before converting dto to entity");
            if (DefaultScenario == null)
                throw new InvalidOperationException("You need to set default scenario before converting dto to entity");
        }
        
    }
}