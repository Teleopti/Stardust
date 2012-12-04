using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command import forecasts into a skill, command result indicates the id of the job result to monitor for import progress.
    /// </summary>
    /// <remarks>The user executing this command must have permission to do File Import.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ImportForecastsFileCommandDto : CommandDto
    {
        /// <summary>
        /// Three options of importing forecasts, i.e. import workload, staffing or both.
        /// </summary>
        [DataMember]
        public ImportForecastsOptionsDto ImportForecastsMode { get; set; }

        /// <summary>
        /// The mandatory id of the uploaded file.
        /// </summary>
        /// <remarks>The file must be uploaded to the database before this command is executed. Only possible to do direct to database.</remarks>
        [DataMember]
        public Guid UploadedFileId { get; set; }

        /// <summary>
        /// The mandatory target skill for the import.
        /// </summary>
        [DataMember]
        public Guid TargetSkillId { get; set; }
    }
}
