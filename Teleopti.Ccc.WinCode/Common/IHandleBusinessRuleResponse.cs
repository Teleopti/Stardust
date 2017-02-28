using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Handles business rules interaction with user
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-05
    /// </remarks>
    public interface IHandleBusinessRuleResponse
    {
        /// <summary>
        /// Sets the response from business rule validation.
        /// </summary>
        /// <param name="businessRulesResponse">The business rules respone.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-19
        /// </remarks>
        void SetResponse(IEnumerable<IBusinessRuleResponse> businessRulesResponse);

        /// <summary>
        /// Gets a value indicating whether [apply to all].
        /// </summary>
        /// <value><c>true</c> if [apply to all]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-24
        /// </remarks>
        bool ApplyToAll { get;}

        /// <summary>
        /// Gets the dialog result.
        /// </summary>
        /// <value>The dialog result.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-05
        /// </remarks>
        DialogResult DialogResult { get; }
    }
}