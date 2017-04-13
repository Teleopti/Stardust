using System;

namespace Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public class WizardNameChangedEventArgs : EventArgs
    {
        private string _newName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardNameChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newName">The new name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public WizardNameChangedEventArgs(string newName)
        {
            _newName = newName;
        }

        /// <summary>
        /// Gets the new name.
        /// </summary>
        /// <value>The new name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public string NewName
        {
            get { return _newName; }
        }
    }
}
