using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonDayOffAssembler : ScheduleDataAssembler<IPersonAssignment, PersonDayOffDto>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public PersonDayOffAssembler(IAssembler<IPerson,PersonDto> personAssembler, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _personAssembler = personAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

				public override PersonDayOffDto DomainEntityToDto(IPersonAssignment entity)
				{
					PersonDayOffDto retDto = null;
					var dayOff = entity.DayOff();
					if (dayOff != null)
					{
					    retDto = new PersonDayOffDto();
						retDto.Id = entity.Id;
						retDto.Person = _personAssembler.DomainEntityToDto(entity.Person);
						retDto.Anchor = dayOff.Anchor;
						retDto.AnchorTime = dayOff.AnchorLocal(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).TimeOfDay;
						retDto.Color = new ColorDto(dayOff.DisplayColor);
						retDto.Length = dayOff.TargetLength;
						retDto.Flexibility = dayOff.Flexibility;
						retDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period.ChangeEndTime(TimeSpan.FromTicks(-1)));
						retDto.Name = dayOff.Description.Name;
						retDto.ShortName = dayOff.Description.ShortName;
						retDto.PayrollCode = dayOff.PayrollCode;
						retDto.Version = entity.Version.GetValueOrDefault(0);						
					}
					return retDto;
        }

				protected override IPersonAssignment DtoToDomainEntityAfterValidation(PersonDayOffDto dto)
        {
					//can't persist schedulepartdto any longer so this doesn't matter (hopefully)
	        return null;
        }
    }
}
