using System.Collections.Generic;

namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// Represents a Class that handles the Smart part visualization requests
    /// </summary>
    public static class SmartPartInvoker
    {
        /// <summary>
        /// holds reference to Smart part command object that Invoker currently handling
        /// </summary>
        private static SmartPartCommand _smartPartCommand;

        /// <summary>
        /// Gets or sets the smart part command.
        /// </summary>
        /// <value>The smart part command.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-11
        /// </remarks>
        public static SmartPartCommand SmartPartCommand
        {
            get { return _smartPartCommand; }
            set { _smartPartCommand = value; }
        }

        /// <summary>
        /// Shows the smart part.
        /// </summary>
        /// <param name="smartPartInfo">The smart part info.</param>
        /// <param name="parameters">The parameters.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public static void ShowSmartPart(SmartPartInformation smartPartInfo, IList<SmartPartParameter> parameters)
        {
            //TODO: validate smartPartInfo object prior to load
            _smartPartCommand.ShowSmartPart(smartPartInfo, parameters);
        }

        /// <summary>
        /// Refreshes the smart part.
        /// </summary>
        /// <param name="smartPartId">The smart part id.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public static void RefreshSmartPart(string smartPartId)
        {
            _smartPartCommand.RefreshSmartPart(smartPartId);
        }

        public static void ClearAllSmartParts()
        {
            _smartPartCommand.ClearAllSmartParts();
        }
    }
}
