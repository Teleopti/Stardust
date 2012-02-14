using System;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;

namespace Teleopti.Ccc.AgentPortal.Main
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ScoreCard")]
    public partial class ScoreCardControl : BaseUserControl
    {
        public ScoreCardControl()
        {
            InitializeComponent();
            if(!this.DesignMode) SetTexts();
            SetToolTip();
        }

        /// <summary>
        /// Gets the web browser.
        /// </summary>
        /// <value>The web browser.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 5/16/2008
        /// </remarks>
        public WebBrowser WebBrowser
        {
            get { return webBrowser1; }
        }

        /// <summary>
        /// Handles the DoubleClick event of the toolStripScoreCards control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 5/16/2008
        /// </remarks>
        private void toolStripScoreCards_DoubleClick(object sender, EventArgs e)
        {
            MainScreen mainScreen = this.ParentForm as MainScreen;
            if (mainScreen != null)
            {
                ViewConstructor.Instance.BuildPortalView(ViewType.Scorecard, mainScreen, mainScreen);
            }
        }

        /// <summary>
        /// Sets the tool tip.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-21
        /// </remarks>
        private void SetToolTip()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(toolStripScoreCards, UserTexts.Resources.ScoreCardPreviewHeaderToolTip);
        }
    }
}
