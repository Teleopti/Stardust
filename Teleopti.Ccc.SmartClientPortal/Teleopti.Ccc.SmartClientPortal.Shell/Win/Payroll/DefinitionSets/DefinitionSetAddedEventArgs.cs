using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    /// <summary>
    /// Custom event arg for the DefinitionSetAddedEventArgs event occurs
    /// </summary>
    public class DefinitionSetAddedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets or sets the definition set.
        /// </summary>
        /// <value>The definition set.</value>
        public MultiplicatorDefinitionSet DefinitionSet
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the definition set view model.
        /// </summary>
        /// <value>The definition set view model.</value>
        public IDefinitionSetViewModel DefinitionSetViewModel
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionSetAddedEventArgs"/> class.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        public DefinitionSetAddedEventArgs(MultiplicatorDefinitionSet definitionSet)
        {
            DefinitionSet = definitionSet;
            DefinitionSetViewModel = new DefinitionSetViewModel(definitionSet);
        }

        #endregion
    }
}
