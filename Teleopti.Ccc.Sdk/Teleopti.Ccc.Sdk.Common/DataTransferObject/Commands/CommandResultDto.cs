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
		/// <summary>
		/// The number of items affected by this command if applicable.
		/// </summary>
        [DataMember]
        public int AffectedItems { get; set; }

		/// <summary>
		/// An optional Id affected by this command.
		/// </summary>
		/// <remarks>The most common use case for this is if the command adds new information. In that case the Id can be used to correlate further actions.</remarks>
        [DataMember]
        public Guid? AffectedId { get; set; }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
    }
}