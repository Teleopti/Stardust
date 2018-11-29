using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IProjectedLayerAssembler : IAssembler<IVisualLayer,ProjectedLayerDto>
    {
        void SetCurrentProjection(IVisualLayerCollection visualLayerCollection, IPerson assignedAgent);
    }

    public class ProjectedLayerAssembler : Assembler<IVisualLayer,ProjectedLayerDto>, IProjectedLayerAssembler
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private IVisualLayerCollection _visualLayerCollection;
		private IPerson _assignedAgent;

		public ProjectedLayerAssembler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override ProjectedLayerDto DomainEntityToDto(IVisualLayer entity)
        {
	        var layerForPeriod = _visualLayerCollection.FilterLayers(entity.Period);
            var layer = new ProjectedLayerDto
                            {
                                Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period),
                                DisplayColor = new ColorDto(entity.Payload.ConfidentialDisplayColor(_assignedAgent)),
                                Description = entity.Payload.ConfidentialDescription(_assignedAgent).Name,
                                ContractTime = layerForPeriod.ContractTime(),
								WorkTime = layerForPeriod.WorkTime(),
								PaidTime = layerForPeriod.PaidTime(),
                                IsAbsence = false,
                                PayloadId = entity.Payload.Id.GetValueOrDefault()
                            };
            
            if (entity.Payload is IAbsence)
                layer.IsAbsence = true;

            IMeetingPayload meetingPayload = entity.Payload as IMeetingPayload;
            if (meetingPayload != null)
            {
                layer.MeetingId = meetingPayload.Meeting.Id;
                layer.PayloadId = meetingPayload.Meeting.Activity.Id.GetValueOrDefault();
            }
            if (entity.DefinitionSet != null)
            {
                layer.Description = string.Concat(layer.Description, ", ", entity.DefinitionSet.Name);
			    layer.OvertimeDefinitionSetId = entity.DefinitionSet.Id.GetValueOrDefault(Guid.Empty);
            }
            return layer;
        }

        public override IVisualLayer DtoToDomainEntity(ProjectedLayerDto dto)
        {
            throw new NotSupportedException("Not supported to create projected layers from DTO.");
        }

        public void SetCurrentProjection(IVisualLayerCollection visualLayerCollection, IPerson assignedAgent)
        {
            _visualLayerCollection = visualLayerCollection;
			_assignedAgent = assignedAgent;
		}
    }
}