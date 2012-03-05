using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command import forecasts into a skill, command result indicates the id of the uploaded file.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/03/")]
    public class ImportForecastsFileCommandDto : CommandDto
    {
        [DataMember]
        public ImportForecastsOptionsDto ImportForecastsOption { get; set; }
        
        [DataMember]
        public Guid UploadedFileId { get; set; }
    }
}
