using System;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonAbsenceAssembler : ScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto>
    {
        private readonly IAssembler<IAbsence, AbsenceDto> _absenceAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public PersonAbsenceAssembler(IAssembler<IAbsence, AbsenceDto> assembler, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _absenceAssembler = assembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override PersonAbsenceDto DomainEntityToDto(IPersonAbsence entity)
        {
            return new PersonAbsenceDto
            {
                Id = entity.Id.GetValueOrDefault(Guid.Empty),
                Version = entity.Version.GetValueOrDefault(0),
                AbsenceLayer = CreateAbsenceLayerDto(entity.Layer)
            };
        }

        private AbsenceLayerDto CreateAbsenceLayerDto(IAbsenceLayer layer)
        {
            var absenceLayer = new AbsenceLayerDto
                                   {
                                       Id = null, //cannot remove because of backward compability
                                       Period = _dateTimePeriodAssembler.DomainEntityToDto(layer.Period),
                                       Absence = _absenceAssembler.DomainEntityToDto(layer.Payload)
                                   };
            return absenceLayer;
        }

        protected override IPersonAbsence DtoToDomainEntityAfterValidation(PersonAbsenceDto dto)
        {
            IAbsenceLayer layer = createLayer(dto.AbsenceLayer);
            IPersonAbsence ret = new PersonAbsence(Person, DefaultScenario, layer);
            //hack
            typeof(AggregateRoot_Events_ChangeInfo_Versioned).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ret, dto.Version);
            ret.SetId(dto.Id);
            return ret;
        }

        private IAbsenceLayer createLayer(AbsenceLayerDto dtoLayer)
        {
            IAbsence absence = _absenceAssembler.DtoToDomainEntity(dtoLayer.Absence);
            DateTimePeriod period = _dateTimePeriodAssembler.DtoToDomainEntity(dtoLayer.Period);

            IAbsenceLayer layer = new AbsenceLayer(absence, period);
            return layer;
        }
    }
}