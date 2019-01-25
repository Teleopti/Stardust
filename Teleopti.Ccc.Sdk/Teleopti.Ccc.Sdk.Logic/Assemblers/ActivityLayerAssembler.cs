using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ActivityLayerAssembler<TLayerType> : Assembler<TLayerType,ActivityLayerDto>, IActivityLayerAssembler<TLayerType>
		where TLayerType : ILayer<IActivity>, IAggregateEntity
    {
        private readonly ILayerConstructor<TLayerType> _layerConstructor;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IAssembler<IActivity, ActivityDto> _activityAssembler;
        private IPerson _person;

        public ActivityLayerAssembler(ILayerConstructor<TLayerType> layerConstructor, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler, IAssembler<IActivity,ActivityDto> activityAssembler)
        {
            _layerConstructor = layerConstructor;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _activityAssembler = activityAssembler;
        }

        public override ActivityLayerDto DomainEntityToDto(TLayerType entity)
        {
        	var period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
            var activity = _activityAssembler.DomainEntityToDto(entity.Payload);
            activity.Description = entity.Payload.ConfidentialDescription_DONTUSE(_person).Name;
            activity.DisplayColor = new ColorDto(entity.Payload.ConfidentialDisplayColor_DONTUSE(_person));

            return new ActivityLayerDto
                       {
                           Id = entity.Id.GetValueOrDefault(),
                           Activity = activity,
                           Period = period
                       };
        }

        public override TLayerType DtoToDomainEntity(ActivityLayerDto dto)
        {
            var period = _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period);
            var activity = _activityAssembler.DtoToDomainEntity(dto.Activity);
            var layer = _layerConstructor.CreateLayer(activity, period);
            layer.SetId(dto.Id);
            return layer;
        }

        public void SetCurrentPerson(IPerson person)
        {
            _person = person;
        }
    }
}