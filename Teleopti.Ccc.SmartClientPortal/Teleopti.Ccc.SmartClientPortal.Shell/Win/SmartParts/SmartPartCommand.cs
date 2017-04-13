using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// Represents a concreate Smart part command class.
    /// </summary>
    public class SmartPartCommand : ISmartPartCommand
    {
        /// <summary>
        /// Holds refernce to smartpart worker
        /// </summary>
        private readonly SmartPartWorker _smartpartWorker;

        /// <summary>
        /// Shows the smart part.
        /// </summary>
        /// <param name="smartPartInfo">The smart part info.</param>
        /// <param name="parameters">The parameters.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public void ShowSmartPart(SmartPartInformation smartPartInfo, IList<SmartPartParameter> parameters)
        {
            _smartpartWorker.ShowSmartPart(smartPartInfo, parameters);
        }

        /// <summary>
        /// Refreshsmarts the part.
        /// </summary>
        /// <param name="smartPartId">The smart part id.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public void RefreshSmartPart(string smartPartId)
        {
            throw new NotImplementedException();
        }

        public void ClearAllSmartParts()
        {
            _smartpartWorker.Workspace.RemoveAllSmartPart();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartPartCommand"/> class.
        /// </summary>
        /// <param name="smartPartWorker">The smart part worker.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public SmartPartCommand(SmartPartWorker smartPartWorker )
        {
            _smartpartWorker = smartPartWorker;
        }
    }
}
