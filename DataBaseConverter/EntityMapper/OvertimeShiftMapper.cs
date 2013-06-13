using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps a PersonalShift.
    /// </summary>
    public class OvertimeShiftMapper : Mapper<OvertimeShift, global::Domain.ShiftBase>
    {
        private readonly DateTime _date;
        private readonly LayerContainsOvertime _layerContainsOvertime;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalShiftMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public OvertimeShiftMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone, DateTime date)
            : base(mappedObjectPair, timeZone)
        {
            _date = date;
            _layerContainsOvertime = new LayerContainsOvertime(mappedObjectPair);
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
        public override OvertimeShift Map(global::Domain.ShiftBase oldEntity)
        {
            OvertimeShift retShift = new OvertimeShiftForConversion();
            ActivityLayerMapper actLayerMapper = new ActivityLayerMapper(MappedObjectPair, ActivityLayerBelongsTo.OvertimeShift, _date, TimeZone);
            foreach (global::Domain.ActivityLayer actLayer in oldEntity.ProjectedLayers().FilterBySpecification(_layerContainsOvertime))
            {
                var newActLayer = actLayerMapper.Map(actLayer);
                if (newActLayer != null)
                    retShift.LayerCollection.Add(newActLayer);
            }
            return retShift;
        }

        private class OvertimeShiftForConversion : OvertimeShift
        {
            public override void OnAdd(ILayer<IActivity> layer)
            {
            }
        }
    }
}