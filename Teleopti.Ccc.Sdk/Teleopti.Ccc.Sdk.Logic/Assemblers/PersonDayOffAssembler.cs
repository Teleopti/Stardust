using System;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonDayOffAssembler : ScheduleDataAssembler<IPersonDayOff, PersonDayOffDto>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public PersonDayOffAssembler(IAssembler<IPerson,PersonDto> personAssembler, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _personAssembler = personAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override PersonDayOffDto DomainEntityToDto(IPersonDayOff entity)
        {
            PersonDayOffDto retDto = new PersonDayOffDto();
            retDto.Id = entity.Id;
            retDto.Person = _personAssembler.DomainEntityToDto(entity.Person);
            retDto.Anchor = entity.DayOff.Anchor;
            retDto.AnchorTime = entity.DayOff.AnchorLocal(TeleoptiPrincipal.Current.Regional.TimeZone).TimeOfDay;
            retDto.Color = new ColorDto(entity.DayOff.DisplayColor);
            retDto.Length = entity.DayOff.TargetLength;
            retDto.Flexibility = entity.DayOff.Flexibility;
            retDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period.ChangeEndTime(TimeSpan.FromTicks(-1)));
            retDto.Name = entity.DayOff.Description.Name;
            retDto.ShortName = entity.DayOff.Description.ShortName;
            retDto.PayrollCode = entity.DayOff.PayrollCode;
            retDto.Version = entity.Version.GetValueOrDefault(0);

            return retDto;
        }

        protected override IPersonDayOff DtoToDomainEntityAfterValidation(PersonDayOffDto dto)
        {

            DayOff dayOff = createDayOff(dto);
            IPersonDayOff ret = new PersonDayOff(Person, DefaultScenario, dayOff, new DateOnly(dto.Anchor.Date), Person.PermissionInformation.DefaultTimeZone());
            ret.SetId(dto.Id);
            //rk - hack
            typeof(AggregateRoot).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ret, dto.Version);
            return ret;
        }

        private static DayOff createDayOff(PersonDayOffDto dto)
        {
            return new DayOff(dto.Anchor,
                            dto.Length,
                            dto.Flexibility,
                            new Description(dto.Name, dto.ShortName),
                            dto.Color.ToColor(), dto.PayrollCode);
        }
    }
}
