﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class OvertimeLayerAssembler : Assembler<IOvertimeShiftActivityLayer,OvertimeLayerDto>, IOvertimeLayerAssembler
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IAssembler<IActivity, ActivityDto> _activityAssembler;
        private IPerson _person;
        private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

        public OvertimeLayerAssembler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IAssembler<IActivity, ActivityDto> activityAssembler, IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
            _activityAssembler = activityAssembler;
        }

        public override OvertimeLayerDto DomainEntityToDto(IOvertimeShiftActivityLayer entity)
        {
        	var period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
            var activity = _activityAssembler.DomainEntityToDto(entity.Payload);
            activity.Description = entity.Payload.ConfidentialDescription(_person,new DateOnly(period.LocalStartDateTime)).Name;
            activity.DisplayColor = new ColorDto(entity.Payload.ConfidentialDisplayColor(_person,new DateOnly(period.LocalStartDateTime)));

            return new OvertimeLayerDto
                       {
                           Id = entity.Id.GetValueOrDefault(),
                           Period = period,
                           OvertimeDefinitionSetId = entity.DefinitionSet.Id.GetValueOrDefault(Guid.Empty),
                           Activity = activity,
                       };
        }

        public override IOvertimeShiftActivityLayer DtoToDomainEntity(OvertimeLayerDto dto)
        {
            IActivity activity = _activityAssembler.DtoToDomainEntity(dto.Activity);
            IMultiplicatorDefinitionSet definitionSet =
                _multiplicatorDefinitionSetRepository.Load(dto.OvertimeDefinitionSetId);
            IOvertimeShiftActivityLayer activityLayer = new OvertimeShiftActivityLayer(activity,
                                                                                       _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period),
                                                                                       definitionSet);
            activityLayer.SetId(dto.Id);

            return activityLayer;
        }

        public void SetCurrentPerson(IPerson person)
        {
            _person = person;
        }
    }
}