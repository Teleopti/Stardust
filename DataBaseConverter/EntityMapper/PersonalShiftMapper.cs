using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps a PersonalShift.
    /// </summary>
    public class PersonalShiftMapper : Mapper<IEnumerable<ILayer<IActivity>>, global::Domain.FillupShift>
    {
        private readonly DateTime _date;

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
        public PersonalShiftMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone, DateTime date) : base(mappedObjectPair, timeZone)
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
				public override IEnumerable<ILayer<IActivity>> Map(global::Domain.FillupShift oldEntity)
        {
            var retShift = new List<ILayer<IActivity>>();
            ActivityLayerMapper actLayerMapper = new ActivityLayerMapper(MappedObjectPair, ActivityLayerBelongsTo.PersonalShift, _date, TimeZone);
            foreach (global::Domain.ActivityLayer actLayer in oldEntity.ProjectedLayers())
            {
                var newActLayer = actLayerMapper.Map(actLayer);
                if (newActLayer != null)
                    retShift.Add(newActLayer);
            }
            return retShift;
        }
    }
}