using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Handles converting from 6x to new format of Absence
    /// </summary>
    public class DayOffMapper : Mapper<IDayOffTemplate, global::Domain.Absence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOffMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        public DayOffMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
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
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        public override IDayOffTemplate Map(global::Domain.Absence oldEntity)
        {
            IDayOffTemplate newDayOff = null;
            if (oldEntity.UseCountRules)
            {
                string oldName = oldEntity.Name;
                if (String.IsNullOrEmpty(oldName))
                    oldName = MissingData.Name;

                while (newDayOff == null)
                {
                    try
                    {
                        newDayOff = new DayOffTemplate(new Description(oldName, oldEntity.ShortName));
                    }
                    catch (ArgumentException)
                    {
                        oldName = oldName.Remove(oldName.Length - 1);
                        newDayOff = null;
                    }
                }
                
                newDayOff.Anchor = TimeSpan.FromHours(12);
                newDayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(0));
                
                if (oldEntity.Deleted)
                    ((IDeleteTag)newDayOff).SetDeleted();
                if (oldEntity.AbsenceActivity != null)
                {
                    MappedObjectPair.AbsenceActivity.Add(oldEntity, oldEntity.AbsenceActivity);
                }
            }

            return newDayOff;
        }
    }
}