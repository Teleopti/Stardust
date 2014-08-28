using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Common.UI.SmartPartControls.SmartParts
{
    /// <summary>
    /// Represents a class that holds environmental information of smart part framework.
    /// </summary>
    public static class SmartPartEnvironment
    {
        /// <summary>
        /// holds reference to Message Broker Instance in Raptor
        /// </summary>
        public static IMessageBroker MessageBroker { get; set; }

        /// <summary>
        /// Gets or sets the smart part workspace.
        /// </summary>
        /// <value>The smart part workspace.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-07
        /// </remarks>
        public static GridWorkspace SmartPartWorkspace { get; set; }
    }
}
