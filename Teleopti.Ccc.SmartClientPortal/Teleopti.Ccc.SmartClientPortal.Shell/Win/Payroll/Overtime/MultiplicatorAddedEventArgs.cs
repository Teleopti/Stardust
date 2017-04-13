using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Overtime
{
    /// <summary>
    /// Custom event arg for the DefinitionSetAddedEventArgs event occurs
    /// </summary>
    public class MultiplicatorAddedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets or sets the definition set.
        /// </summary>
        /// <value>The definition set.</value>
        public IMultiplicator Multiplicator
        {
            get;
            private set;
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionSetAddedEventArgs"/> class.
        /// </summary>
        /// <param name="multiplicator">The multiplicator.</param>
        public MultiplicatorAddedEventArgs(IMultiplicator multiplicator)
        {
            Multiplicator = multiplicator;
        }

        #endregion
    }
}
