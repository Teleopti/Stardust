using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command import forecasts into a skill, command result indicates the id of the uploaded file.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ImportForecastsFileCommandDto : CommandDto
    {
        /// <summary>
        /// Three options of importing forecasts, i.e. import workload, staffing or both.
        /// </summary>
        [DataMember]
        public ImportForecastsOptionsDto ImportForecastsMode { get; set; }

        /// <summary>
        /// The id of the uploaded file.
        /// </summary>
        [DataMember]
        public Guid UploadedFileId { get; set; }

        /// <summary>
        /// The target skill to be imported to
        /// </summary>
        [DataMember]
        public Guid TargetSkillId { get; set; }
    }
}
