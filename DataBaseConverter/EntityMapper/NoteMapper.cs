using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using OldAgentDay = Domain.AgentDay;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapper for notes
    /// </summary>
    public class NoteMapper : Mapper<INote, OldAgentDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        public NoteMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone)
            : base(mappedObjectPair, timeZone)
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
        public override INote Map(OldAgentDay oldEntity)
        {
            if (!string.IsNullOrEmpty(oldEntity.Note))
            {
                IPerson newPerson = MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent);
                IScenario newScenario = MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario);
                INote note = new Note(newPerson, new DateOnly(TimeZoneHelper.ConvertFromUtc(oldEntity.AgentDate, TimeZone)), newScenario, oldEntity.Note);
                return note;
            }
            return null;
        }
    }
}
