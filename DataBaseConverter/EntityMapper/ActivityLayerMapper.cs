using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an old activity layer
    /// </summary>
    public class ActivityLayerMapper : Mapper<ILayer<IActivity>, global::Domain.ActivityLayer>
    {
        private readonly DateTime _date;
        private readonly ActivityLayerBelongsTo _typeOfLayer;
        private readonly LayerContainsOvertime _layerContainsOvertime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLayerMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="typeOfLayer">The type of layer.</param>
        /// <param name="date">The date.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public ActivityLayerMapper(MappedObjectPair mappedObjectPair,
                                    ActivityLayerBelongsTo typeOfLayer,
                                    DateTime date,
                                    TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
            _layerContainsOvertime = new LayerContainsOvertime(mappedObjectPair);
            _typeOfLayer = typeOfLayer;
            _date = date;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
				public override ILayer<IActivity> Map(global::Domain.ActivityLayer oldEntity)
        {
            IMultiplicatorDefinitionSet definitionSet = null;
            DateTimePeriodMapper dtpMap = new DateTimePeriodMapper(TimeZone,_date);
            IActivity newAct = MappedObjectPair.ResolveActivity(oldEntity.LayerActivity);
            if (newAct == null) return null;
            
            DateTimePeriod newPeriod = dtpMap.Map(oldEntity.Period);
						ILayer<IActivity> returnLayer = null;
            switch (_typeOfLayer)
            {
                case ActivityLayerBelongsTo.MainShift:
                    if (!_layerContainsOvertime.IsSatisfiedBy(oldEntity))
                        returnLayer = new EditableShiftLayer(newAct, newPeriod);
                    break;
                case ActivityLayerBelongsTo.PersonalShift:
                    if (!_layerContainsOvertime.IsSatisfiedBy(oldEntity))
                        returnLayer = new PersonalShiftLayer(newAct, newPeriod);
                    break;
                case ActivityLayerBelongsTo.WorkShift:
                    if (!_layerContainsOvertime.IsSatisfiedBy(oldEntity))
                        returnLayer = new WorkShiftActivityLayer(newAct, newPeriod);
                    break;
                case ActivityLayerBelongsTo.OvertimeShift:
                    var overtime = MappedObjectPair.OvertimeActivity.GetPaired(oldEntity.LayerActivity);
                    if (overtime != null)
                    {
                        definitionSet = MappedObjectPair.MultiplicatorDefinitionSet.GetPaired(overtime);
                    }

                    returnLayer = new OvertimeShiftLayer(newAct, newPeriod, definitionSet);
                    break;
            }
            return returnLayer;
        }
    }
}