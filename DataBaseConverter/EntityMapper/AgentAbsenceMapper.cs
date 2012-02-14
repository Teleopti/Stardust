using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using OldAgentDay = global::Domain.AgentDay;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an AgentDay to an PersonAbsence
    /// </summary>
    public class AgentAbsenceMapper : Mapper<IPersonAbsence, OldAgentDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAbsenceMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentAbsenceMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
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
        public override IPersonAbsence Map(global::Domain.AgentDay oldEntity)
        {
            IPersonAbsence agAbs = null;

            if ((oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithoutSavedWorkShift ||
                 oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithSavedWorkShift) &&
                (!oldEntity.AgentDayAssignment.Assigned.AssignedAbsence.UseCountRules))
            {
                DateTimePeriod period = new DateTimePeriod(TimeZone.ConvertTimeToUtc(DateTime.SpecifyKind(oldEntity.AgentDate, DateTimeKind.Unspecified), TimeZone), TimeZone.ConvertTimeToUtc(DateTime.SpecifyKind(oldEntity.AgentDate, DateTimeKind.Unspecified), TimeZone).AddDays(1));
                agAbs = new PersonAbsence(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent), MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario), new AbsenceLayer(MappedObjectPair.Absence.GetPaired(oldEntity.AgentDayAssignment.Assigned.AssignedAbsence), period));
            }

            return agAbs;
        }

    }
}