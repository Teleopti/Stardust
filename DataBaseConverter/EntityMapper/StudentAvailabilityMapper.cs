using System;
using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    ///<summary>
    /// Maps old student availability to new StudentAvailabilityDay
    ///</summary>
    public class StudentAvailabilityMapper : Mapper<IStudentAvailabilityDay, AgentDay>
    {
        ///<summary>
        /// Initializes a new instance of the <see cref="StudentAvailabilityMapper"/> class.
        ///</summary>
        ///<param name="mappedObjectPair"></param>
        public StudentAvailabilityMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IStudentAvailabilityDay Map(AgentDay oldEntity)
        {
            if (oldEntity.Limitation == null || oldEntity.Limitation.WebCoreTime == null)
                return null;

            IPerson person = findPerson(oldEntity.AssignedAgent.Id);
            return new StudentAvailabilityDay(person, new DateOnly(oldEntity.AgentDate), buildRestrictions(oldEntity));
        }

        private static IList<IStudentAvailabilityRestriction> buildRestrictions(AgentDay oldEntity)
        {
            var restrictionCollection = new List<IStudentAvailabilityRestriction>();

            foreach (OpenLayer openLayer in oldEntity.Limitation.WebCoreTime)
            {
                var restriction = new StudentAvailabilityRestriction
                                                                 {
                                                                     StartTimeLimitation =
                                                                         new StartTimeLimitation(openLayer.Period.StartTime, null),
                                                                     EndTimeLimitation = new EndTimeLimitation(null, openLayer.Period.EndTime)
                                                                 };
                restrictionCollection.Add(restriction);
            }

            return restrictionCollection;
        }

        private IPerson findPerson(int oldIdOnPerson)
        {
            IPerson ret = null;
            foreach (ObjectPair<Agent, IPerson> pair in MappedObjectPair.Agent)
            {
                if (pair.Obj1.Id == oldIdOnPerson)
                    ret = pair.Obj2;
            }
            return ret;
        }
    }
}
