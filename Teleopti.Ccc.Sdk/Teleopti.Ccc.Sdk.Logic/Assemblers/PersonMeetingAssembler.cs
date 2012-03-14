﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonMeetingAssembler : ScheduleDataAssembler<IPersonMeeting, PersonMeetingDto>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public PersonMeetingAssembler(IAssembler<IPerson,PersonDto> personAssembler,IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _personAssembler = personAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override PersonMeetingDto DomainEntityToDto(IPersonMeeting entity)
        {
            PersonMeetingDto personMeetingDto = new PersonMeetingDto();
            personMeetingDto.Id = entity.Id;
            personMeetingDto.Person = _personAssembler.DomainEntityToDto(entity.Person);
            personMeetingDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
            personMeetingDto.Subject = entity.BelongsToMeeting.Subject;
            personMeetingDto.Location = entity.BelongsToMeeting.Location;
            personMeetingDto.Description = entity.BelongsToMeeting.Description;
            personMeetingDto.MeetingId = entity.BelongsToMeeting.Id.GetValueOrDefault(Guid.Empty);
            return personMeetingDto;
        }

        protected override IPersonMeeting DtoToDomainEntityAfterValidation(PersonMeetingDto dto)
        {
            var period = _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period);
            IPersonMeeting personMeeting = new PersonMeeting(null, null,period);
            personMeeting.SetId(dto.Id);
            return personMeeting;
        }
    }
}
