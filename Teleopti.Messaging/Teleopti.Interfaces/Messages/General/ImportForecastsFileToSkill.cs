using System;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Message with details of importing forecasts to skill
    /// </summary>
    public class ImportForecastsFileToSkill : MessageWithLogOnInfo
    {
        ///<summary>
        /// The Id of the job this message will feed with updates.
        ///</summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// The identity of this message.
        /// </summary>
        public override Guid Identity
        {
            get { return JobId; }
        }

        /// <summary>
        /// The id of the uploaded file.
        /// </summary>
        public Guid UploadedFileId { get; set; }

        /// <summary>
        /// The target skill to be imported to
        /// </summary>
        public Guid TargetSkillId { get; set; }

        /// <summary>
        /// The owner of this action.
        /// </summary>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// Mode of importing forecasts
        /// </summary>
        public ImportForecastsMode ImportMode { get; set; }
    }

    /// <summary>
    /// Provide choices regarding importing forecasts
    /// </summary>
    public enum ImportForecastsMode
    {
         /// <summary>
        /// Import workload only
        /// </summary>
        ImportWorkload,
        /// <summary>
        /// Import staffing only
        /// </summary>
        ImportStaffing,
        /// <summary>
        /// Import both workload and staffing
        /// </summary>
        ImportWorkloadAndStaffing
    }
}