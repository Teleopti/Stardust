using System;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Message with details of importing forecasts to skill
    /// </summary>
    public class ImportForecastsFileToSkill : RaptorDomainMessage
    {
        ///<summary>
        /// The Id of the job this message will feed with updates.
        ///</summary>
        public Guid JobId { get; set; }

        public override Guid Identity
        {
            get { return JobId; }
        }

        /// <summary>
        /// The target skill to be imported to
        /// </summary>
        public Guid TargetSkillId { get; set; }

        /// <summary>
        /// The owner of this action.
        /// </summary>
        public Guid OwnerPersonId { get; set; }
    }
}