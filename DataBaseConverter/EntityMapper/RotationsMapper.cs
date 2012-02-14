using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps Rotations
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-24
    ///  Updated by : MadhurangaP
    ///  Updated Date : 2008-11-27    
    /// /// </remarks>
    public class RotationsMapper : Mapper<IRotation, DataRow>
    {
        private readonly IList<DataRow> _rotationDays;
        private readonly int _intervalLength;
        private readonly IList<IDayOffTemplate> _dayOffs;
        /// <summary>
        /// Initializes a new instance of the <see cref="RotationsMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="rotationDays">The rotation days.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <param name="dayOffs">The day offs.</param>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-09-24
        /// /// </remarks>
        public RotationsMapper(MappedObjectPair mappedObjectPair, IList<DataRow> rotationDays, int intervalLength, IList<IDayOffTemplate> dayOffs)
            : base(mappedObjectPair, null)
        {
            _rotationDays = rotationDays;
            _intervalLength = intervalLength;
            _dayOffs = dayOffs;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IRotation Map(DataRow oldEntity)
        {
            int id = (int)oldEntity["rotation_id"];
            IRotation newRotation = new Rotation((string)oldEntity["rotation_name"], (int)oldEntity["weeks"] * 7);

            IList<DataRow> theDays = RotationDaysOnRotation(id);
            foreach (DataRow day in theDays)
            {
                int dayIdx = (int)day["rotation_day"] - 1;

                int startInterval = 0;
                int startMinute = 0;
                if (day["start_interval"] != DBNull.Value)
                {
                    startInterval = (int)day["start_interval"];
                    startMinute = startInterval * _intervalLength;
                }

                int endMinute = 0;
                if (day["end_interval"] != DBNull.Value)
                {
                    int endInterval = (int)day["end_interval"] + 1;
                    endMinute = endInterval * _intervalLength;
                    // next day
                    if (endInterval < startInterval)
                        endMinute = endMinute + 1440;
                }

                int shiftCat = (int)day["shift_cat_id"];

                int? absence = null;
                if (day["abs_id"] != DBNull.Value)
                    absence = (int)day["abs_id"];

                var newRestriction = new RotationRestriction();
                newRestriction.DayOffTemplate = (absence.HasValue) ? GetDayOff(day["abs_short_desc"].ToString()) : null;

                if (startMinute > 0)
                    newRestriction.StartTimeLimitation =
                        new StartTimeLimitation(TimeSpan.FromMinutes(startMinute), TimeSpan.FromMinutes(startMinute));
                else
                {
                    newRestriction.StartTimeLimitation = new StartTimeLimitation();
                }
                if (endMinute > 0)
                    newRestriction.EndTimeLimitation =
                        new EndTimeLimitation(TimeSpan.FromMinutes(endMinute), TimeSpan.FromMinutes(endMinute));
                else
                {
                    newRestriction.EndTimeLimitation = new EndTimeLimitation();
                }
                IShiftCategory cat = FindShiftCategory(shiftCat);
                if (cat != null && !absence.HasValue)
                    newRestriction.ShiftCategory = cat;

                newRotation.RotationDays[dayIdx].RestrictionCollection[0].StartTimeLimitation =  newRestriction.StartTimeLimitation;
                newRotation.RotationDays[dayIdx].RestrictionCollection[0].EndTimeLimitation = newRestriction.EndTimeLimitation;
                newRotation.RotationDays[dayIdx].RestrictionCollection[0].WorkTimeLimitation = newRestriction.WorkTimeLimitation;
                newRotation.RotationDays[dayIdx].RestrictionCollection[0].ShiftCategory = newRestriction.ShiftCategory;
                newRotation.RotationDays[dayIdx].RestrictionCollection[0].DayOffTemplate = newRestriction.DayOffTemplate;

            }
            return newRotation;
        }

        /// <summary>
        /// Gets the day off.
        /// </summary>
        /// <param name="dayOffName">Name of the day off.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-27
        /// </remarks>
        private IDayOffTemplate GetDayOff(IEquatable<string> dayOffName)
        {
            return _dayOffs.FirstOrDefault(d => dayOffName.Equals(d.Description.ShortName));
        }

        private IShiftCategory FindShiftCategory(int oldIdOnCategory)
        {
            if (oldIdOnCategory == -1) return null;

            var oldCategory =
                MappedObjectPair.ShiftCategory.Obj1Collection().FirstOrDefault(c => c.Id == oldIdOnCategory);
            if (oldCategory == null) return null;

            return MappedObjectPair.ShiftCategory.GetPaired(oldCategory);
        }

        private IList<DataRow> RotationDaysOnRotation(int rotationId)
        {
            IList<DataRow> ret = new List<DataRow>();
            foreach (DataRow row in _rotationDays)
            {
                if ((int)row["rotation_id"] == rotationId)
                    ret.Add(row);
            }
            return ret;
        }
    }
}
