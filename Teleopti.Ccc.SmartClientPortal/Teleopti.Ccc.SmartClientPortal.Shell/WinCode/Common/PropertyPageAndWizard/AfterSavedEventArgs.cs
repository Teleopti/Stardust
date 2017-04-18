using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Event args for use after saved
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class AfterSavedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the saved aggregate root.
        /// </summary>
        /// <value>The saved aggregate root.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public IAggregateRoot SavedAggregateRoot { get; set; }
    }
}
