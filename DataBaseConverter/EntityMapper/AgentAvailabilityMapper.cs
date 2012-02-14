using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an AgentDay to an PersonAvailability
    /// </summary>
    public class AgentAvailabilityMapper : Mapper<PersonAvailability, global::Domain.AgentDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAvailabilityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentAvailabilityMapper(MappedObjectPair mappedObjectPair,
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
        public override PersonAvailability Map(global::Domain.AgentDay oldEntity)
        {
            PersonAvailability agAvail = null;

            if (oldEntity.Limitation != null &&
                oldEntity.Limitation.CoreTime != null)
            {
                DateTimePeriod period =
                    new DateTimePeriod(
                        TimeZoneInfo.ConvertTimeToUtc(
                            DateTime.SpecifyKind(oldEntity.AgentDate.Add(oldEntity.Limitation.CoreTime.Period.StartTime),DateTimeKind.Unspecified),
                            TimeZone),
                        TimeZoneInfo.ConvertTimeToUtc(
                            DateTime.SpecifyKind(oldEntity.AgentDate.Add(oldEntity.Limitation.CoreTime.Period.EndTime), DateTimeKind.Unspecified), TimeZone));
                agAvail = new PersonAvailability(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
                                                MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario));

                //agAvail.LayerCollection.Add(
                //    new AvailabilityLayer(_defaultAvailability,
                //                          period));
            }

            return agAvail;
        }
    }
}