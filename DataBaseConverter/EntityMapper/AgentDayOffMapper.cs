using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an AgentDay to an PersonAbsence
    /// </summary>
    public class AgentDayOffMapper : Mapper<IPersonDayOff, global::Domain.AgentDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDayOffMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentDayOffMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
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
        public override IPersonDayOff Map(global::Domain.AgentDay oldEntity)
        {
            PersonDayOff agDayOff = null;

            if ((oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithoutSavedWorkShift ||
                 oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithSavedWorkShift) &&
                (oldEntity.AgentDayAssignment.Assigned.AssignedAbsence.UseCountRules))
            {
                agDayOff = new PersonDayOff(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
                                            MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario),
                                            MappedObjectPair.DayOff.GetPaired(oldEntity.AgentDayAssignment.Assigned.AssignedAbsence),
                                            new DateOnly(oldEntity.AgentDate.Date));
            }

            return agDayOff;
        }
    }
}