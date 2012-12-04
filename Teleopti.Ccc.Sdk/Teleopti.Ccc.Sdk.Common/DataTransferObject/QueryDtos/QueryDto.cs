using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{  
    /// <summary>
    /// Base class for a query objects.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    [KnownType("GetKnownTypes")]
    public class QueryDto  : IExtensibleDataObject
	{
		[NonSerialized]
		private ExtensionDataObject _extensionData;

        /// <summary>
        /// Get the known types of <see cref="QueryDto"/>.
        /// </summary>
        /// <remarks>This method is intended for internal usage only.</remarks>
        /// <returns>The types derived from <see cref="QueryDto"/>.</returns>
        public static IEnumerable<Type> GetKnownTypes()
        {
            return KnownTypesHelper.GetKnownTypes<QueryDto>();
        }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
    	public ExtensionDataObject ExtensionData
    	{
    		get { return _extensionData; }
    		set { _extensionData = value; }
    	}
    }
}
