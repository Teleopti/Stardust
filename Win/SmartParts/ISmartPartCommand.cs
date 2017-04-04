using System.Collections.Generic;

namespace Teleopti.Ccc.Win.SmartParts
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
        /// Refreshsmarts the part.
        /// </summary>
        /// <param name="smartPartId">The smart part id.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        void RefreshSmartPart(string smartPartId);

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
