using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Activity=Domain.Activity;
using OldAgentDay = Domain.AgentDay;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps activitylayers to absencelayers
    /// </summary>
    public class AgentAbsenceActivityMapper : Mapper<IList<IPersonAbsence>, OldAgentDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAbsenceActivityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public AgentAbsenceActivityMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
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
        /// Created by: micke
        /// Created date: 2008-08-09
        /// </remarks>
        public override IList<IPersonAbsence> Map(OldAgentDay oldEntity)
        {
            IList<IPersonAbsence> retList = new List<IPersonAbsence>();
            if (oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.WorkShift)
            {
                IPerson newPerson = MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent);
                IScenario newScenario = MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario);
                DateTimePeriodMapper dtpMap = new DateTimePeriodMapper(TimeZone,oldEntity.AgentDate);

                foreach (global::Domain.ActivityLayer actLayer in oldEntity.AgentDayAssignment.Assigned.AssignedWorkshift.LayerCollection)
                {
                    global::Domain.Absence oldAbsence = AbsenceFromActivity(actLayer.LayerActivity);
                    if (oldAbsence != null)
                    {
                        IAbsenceLayer absenceLayer = new AbsenceLayer(MappedObjectPair.Absence.GetPaired(oldAbsence),
                                                         dtpMap.Map(actLayer.Period));
                        IPersonAbsence agAbs = new PersonAbsence(newPerson, newScenario, absenceLayer);
                        retList.Add(agAbs);
                    }

                }
            }

            return retList;
        }


        private global::Domain.Absence AbsenceFromActivity(Activity oldActivity)
        {
            foreach (ObjectPair<global::Domain.Absence, Activity> pair in MappedObjectPair.AbsenceActivity)
            {
                if (pair.Obj2.Equals(oldActivity))
                    return pair.Obj1;
            }

            return null;
        }

       
    }
}
