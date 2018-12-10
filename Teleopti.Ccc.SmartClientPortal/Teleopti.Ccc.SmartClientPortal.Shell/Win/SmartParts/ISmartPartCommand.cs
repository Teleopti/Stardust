using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    /// <summary>
    /// Defines the functionality of a Smart Part Command.
    /// </summary>
    public interface ISmartPartCommand
    {
        /// <summary>
        /// Shows the smart part.
        /// </summary>
        /// <param name="smartPartInfo">The smart part info.</param>
        /// <param name="parameters">The parameters.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        void ShowSmartPart(SmartPartInformation smartPartInfo, IList<SmartPartParameter> parameters);

        /// <summary>
        /// Clears all smart parts.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-03-10
        /// </remarks>
        void ClearAllSmartParts();
    }
}
