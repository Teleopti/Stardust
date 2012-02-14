using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// Represents a CommandResultDto object, it includes the affected id after the command execution and the amount of affected items.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class CommandResultDto : IExtensibleDataObject
    {
        [DataMember]
        public int AffectedItems { get; set; }

        [DataMember]
        public Guid? AffectedId { get; set; }

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
    }
}