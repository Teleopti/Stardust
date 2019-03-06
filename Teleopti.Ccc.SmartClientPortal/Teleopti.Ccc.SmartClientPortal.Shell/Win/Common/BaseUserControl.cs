using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Base user control for use in Raptor project.
    /// </summary>
    /// <remarks>
    /// Contains logic to perform translation.
    /// Created by: robink
    /// Created date: 2007-12-27
    /// </remarks>
    public class BaseUserControl : UserControl, ILocalized, IHelpContext, IHelpForm
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

		[RemoveMeWithToggle(Toggles.ResourcePlanner_PrepareToRemoveRightToLeft_81112)]
		public void SetTextsNoRightToLeft()
		{
			new LanguageResourceHelper().SetTexts(this, false);
			SetCommonTexts();
		}

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

        /// <summary>
        /// Gets a value indicating whether this instance has help information.
        /// Default is True
        /// </summary>
        /// <value><c>true</c> if this instance has help information; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-01
        /// </remarks>
        public virtual bool HasHelp
        {
            get { return true; }
        }

        public virtual string HelpId
        {
            get { return Name; }
        }

	    public IHelpContext FindMatchingManualHelpContext(Control control)
	    {
		    return null;
	    }

	    /// <summary>
        /// Gets the current UI culture information.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get
            {
                bool designMode;
                using (BaseUserControl baseUserControl = new BaseUserControl())
                {
                    designMode = baseUserControl.DesignMode;
                }
                if (designMode)
                {
                    return CultureInfo.CurrentUICulture;
                }
                return TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;
            }
        }
    }
}
