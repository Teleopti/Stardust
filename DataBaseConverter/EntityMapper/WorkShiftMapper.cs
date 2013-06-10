using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps a WorkShift.
    /// </summary>
    public class WorkShiftMapper : Mapper<IWorkShift, global::Domain.WorkShift>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkShiftMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public WorkShiftMapper(MappedObjectPair mappedObjectPair, 
                               TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
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
        public override IWorkShift Map(global::Domain.WorkShift oldEntity)
        {
            if (oldEntity.PermanentExcluded || oldEntity.Bag == null) return null;

            //todo
            IWorkShift retShift = new WorkShift(MappedObjectPair.ShiftCategory.GetPaired(oldEntity.Category));
            ActivityLayerMapper actLayerMapper = new ActivityLayerMapper(MappedObjectPair, ActivityLayerBelongsTo.WorkShift, WorkShift.BaseDate, TimeZone);
            LayerCollection<IActivity> activityLayerCollection = new LayerCollection<IActivity>();
            foreach (global::Domain.ActivityLayer actLayer in oldEntity.LayerCollection)
            {
                var newLayer = actLayerMapper.Map(actLayer);
                if (newLayer != null)
                    activityLayerCollection.Add(newLayer);
            }
            ((LayerCollection<IActivity>)retShift.LayerCollection).AddRange(activityLayerCollection);

            return retShift;
        }
    }
}