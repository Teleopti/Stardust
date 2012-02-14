using Teleopti.Ccc.AgentPortalCode.Common;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Common
{
    /// <summary>
    /// Base user control for use in Raptor project.
    /// </summary>
    /// <remarks>
    /// Contains logic to perform translation.
    /// Created by: robink
    /// Created date: 2007-12-27
    /// </remarks>
    public class BaseUserControl : UserControl, ILocalized, IHelpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserControl"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        protected BaseUserControl()
        {
        }

        #region ILocalized Members

        /// <summary>
        /// Sets the texts.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        public void SetTexts()
        {
            new LanguageResourceHelper().SetTexts(this);
            SetCommonTexts();
        }

        #endregion

        #region IHelpContext Members

        /// <summary>
        /// Gets a value indicating whether this instance has help information.
        /// Default is True
        /// </summary>
        /// <value><c>true</c> if this instance has help information; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-01
        /// </remarks>
        public bool HasHelp
        {
            get { return true; }
        }

        #endregion

        /// <summary>
        /// Sets the common texts.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        protected virtual void SetCommonTexts()
        {
        }
    }
}
