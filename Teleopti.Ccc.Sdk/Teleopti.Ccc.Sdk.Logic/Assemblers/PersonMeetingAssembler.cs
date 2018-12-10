using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

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
        	var normalizeText = new NormalizeText();
            personMeetingDto.Id = null;
            personMeetingDto.Person = _personAssembler.DomainEntityToDto(entity.Person);
            personMeetingDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
            personMeetingDto.Subject = entity.BelongsToMeeting.GetSubject(normalizeText);
            personMeetingDto.Location = entity.BelongsToMeeting.GetLocation(normalizeText);
            personMeetingDto.Description = entity.BelongsToMeeting.GetDescription(normalizeText);
            personMeetingDto.MeetingId = entity.BelongsToMeeting.Id.GetValueOrDefault(Guid.Empty);
            return personMeetingDto;
        }

        protected override IPersonMeeting DtoToDomainEntityAfterValidation(PersonMeetingDto dto)
        {
            var period = _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period);
            IPersonMeeting personMeeting = new PersonMeeting(null, null,period);
            return personMeeting;
        }
    }
}
