﻿using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PreferenceDayAssembler : ScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto>
    {
        private readonly IAssembler<IPreferenceRestriction, PreferenceRestrictionDto> _restrictionAssembler;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;

        public PreferenceDayAssembler(IAssembler<IPreferenceRestriction, PreferenceRestrictionDto> restrictionAssembler, IAssembler<IPerson, PersonDto> personAssembler)
        {
            _restrictionAssembler = restrictionAssembler;
            _personAssembler = personAssembler;
        }

        public override PreferenceRestrictionDto DomainEntityToDto(IPreferenceDay entity)
        {
            var dto = _restrictionAssembler.DomainEntityToDto(entity.Restriction);
            dto.Person = _personAssembler.DomainEntityToDto(entity.Person);
			dto.RestrictionDate = new DateOnlyDto { DateTime = entity.RestrictionDate };
            dto.MustHave = entity.Restriction.MustHave;
            dto.TemplateName = entity.TemplateName;
            dto.Id = entity.Id;
            return dto;
        }

        protected override void EnsureInjectionForDtoToDo()
        {
        }
        
        protected override IPreferenceDay DtoToDomainEntityAfterValidation(PreferenceRestrictionDto dto)
        {
            var preferenceRestriction = _restrictionAssembler.DtoToDomainEntity(dto);
            IPerson person = _personAssembler.DtoToDomainEntity(dto.Person);
            IPreferenceDay day = new PreferenceDay(person, new DateOnly(dto.RestrictionDate.DateTime), preferenceRestriction);
            day.TemplateName = dto.TemplateName;
            day.Restriction.MustHave = dto.MustHave;
            day.SetId(dto.Id);
            return day;
        }
    }
}
