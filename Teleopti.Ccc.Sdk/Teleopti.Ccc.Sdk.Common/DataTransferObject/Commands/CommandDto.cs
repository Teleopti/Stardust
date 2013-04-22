using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{  
    /// <summary>
    /// Represents a CommandDto object.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    [KnownType("GetKnownTypes")]
    public class CommandDto  : IExtensibleDataObject
	{
		[NonSerialized]
		private ExtensionDataObject _extensionData;

		/// <summary>
		/// Get the known types of <see cref="CommandDto"/>.
		/// </summary>
		/// <remarks>This method is intended for internal usage only.</remarks>
		/// <returns>The types derived from <see cref="CommandDto"/>.</returns>
		public static IEnumerable<Type> GetKnownTypes()
		{
			return KnownTypesHelper.GetKnownTypes<CommandDto>();
		}

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
    	public ExtensionDataObject ExtensionData
    	{
    		get { return _extensionData; }
    		set { _extensionData = value; }
    	}

		/// <exclude />
		/// <summary>
		/// Result from the command. Usable when running synchronously. Is not part of the contract.
		/// </summary>
	    public CommandResultDto Result { get; set; }
	}
}
