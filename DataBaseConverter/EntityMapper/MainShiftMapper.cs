using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps a MainShift.
    /// </summary>
    public class MainShiftMapper : Mapper<IEditableShift, global::Domain.WorkShift>
    {
        private readonly DateTime _date;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainShiftMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public MainShiftMapper( MappedObjectPair mappedObjectPair, 
                                TimeZoneInfo timeZone,
                                DateTime date) : base(mappedObjectPair, timeZone)
        {
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
        public override IEditableShift Map(global::Domain.WorkShift oldEntity)
        {
            IShiftCategory category = MappedObjectPair.ShiftCategory.GetPaired(oldEntity.Category);
            IEditableShift retShift = new EditableShift(category);
            var actLayerMapper = new ActivityLayerMapper(MappedObjectPair, ActivityLayerBelongsTo.MainShift, _date, TimeZone);
            global::Domain.Activity baseActivity;
            if(oldEntity.Bag == null)
                baseActivity = BaseActivity(oldEntity);
            else
                baseActivity = oldEntity.Bag.BaseActivity;
            retShift.LayerCollection.Add(BaseLayer(oldEntity, baseActivity, actLayerMapper));
            foreach (global::Domain.ActivityLayer actLayer in oldEntity.LayerCollection)
            {
                if (!absenceActivity(actLayer.LayerActivity))
                {
                    if (actLayer.LayerActivity != baseActivity)
                    {
						var newLayer = (EditableShiftLayer)actLayerMapper.Map(actLayer);
                        if (newLayer != null)
                            retShift.LayerCollection.Add(newLayer);
                    }
                }
            }

            return retShift;
        }

        private static EditableShiftLayer BaseLayer(global::Domain.WorkShift oldEntity, global::Domain.Activity baseActivity, ActivityLayerMapper actLayerMapper)
        {
            global::Domain.ActivityLayer oldBaseLayer = new global::Domain.ActivityLayer(oldEntity.ProjectedPeriod(), baseActivity);
			return (EditableShiftLayer)actLayerMapper.Map(oldBaseLayer);
        }

        private bool absenceActivity(global::Domain.Activity oldActivity)
        {
            return MappedObjectPair.AbsenceActivity.Obj2Collection().Contains(oldActivity);
        }

        private static global::Domain.Activity BaseActivity(global::Domain.WorkShift oldEntity)
        {
            IDictionary<global::Domain.Activity, int> result = new Dictionary<global::Domain.Activity, int>();
            foreach (global::Domain.ActivityLayer actLayer in oldEntity.LayerCollection)
            {
                int sum;
                if (result.TryGetValue(actLayer.LayerActivity, out sum))
                    sum += 1;
                else result[actLayer.LayerActivity] = 1;
            }
            int max = 0;
            global::Domain.Activity maxActivity = null;
            foreach (KeyValuePair<global::Domain.Activity, int> pair in result)
            {
                if (pair.Value > max)
                {
                    max = pair.Value;
                    maxActivity = pair.Key;
                }
            }
            return maxActivity;
        }
    }
}
