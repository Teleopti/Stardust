using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Defines a Teleopti application function
    /// </summary>
    public interface IApplicationFunction : IAggregateRoot, 
                                                IParentChildEntity,
                                                ICloneable
    {
        /// <summary>
        /// Removes the application role from the context.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <remarks>
        /// You have to call this function before you delete the application function from repository. 
        /// </remarks>
        void RemoveApplicationRoleFromContext(IEnumerable<IApplicationRole> applicationRoles);

        /// <summary>
        /// Gets or sets the function description key.
        /// </summary>
        /// <value>The FunctionDescription value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        string FunctionDescription { get; set; }

        /// <summary>
        /// Gets the localized function description text.
        /// </summary>
        /// <value>The FunctionDescription value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        string LocalizedFunctionDescription { get; }

        /// <summary>
        /// Gets or sets the FunctionCode value.
        /// </summary>
        /// <value>The FunctionCode value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        string FunctionCode { get; set; }

        /// <summary>
        /// Gets the order value. Used for setting the order of the Aaplication Function in the function lists.
        /// </summary>
        /// <value>The order.</value>
        int? SortOrder { get; set; }

        /// <summary>
        /// Gets or sets whether the application funciotn is permitted. It has no value, if the permission value is not defined,
        /// basically if you get the instance from other source than the Authorization Service.
        /// </summary>
        /// <value>The permitted flag.</value>
        bool? IsPermitted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is preliminary. 
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is preliminary; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
		/// By design intention, a preliminary function does not checked against licence and not 
		/// appear on the permitted application list
        /// </remarks>
        bool IsPreliminary { get; set; }

        /// <summary>
        /// Gets the FunctionPath value.
        /// </summary>
        /// <value>The FunctionPath value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        string FunctionPath { get; }

        /// <summary>
        /// Gets or sets the foreign id.
        /// </summary>
        /// <value>The foreign id.</value>
        /// <remarks>
        /// This is the id that the system stores to check if the foreign application function
        /// has been deleted, or changed. It is used together with ForeignSource property
        /// </remarks>
        string ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the foreign source.
        /// </summary>
        /// <value>The foreign source.</value>
        /// <remarks>
        /// Used together with ForeignId property to define a fogeign application function.
        /// </remarks>
        string ForeignSource { get; set; }
    }
}
