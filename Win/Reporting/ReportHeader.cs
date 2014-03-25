using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportHeader : BaseUserControl
    {
        public event EventHandler ShowSettings;
        public event EventHandler HideSettings;

    	public ReportHeader()
        {
            InitializeComponent();

            if (!StateHolderReader.IsInitialized || DesignMode) return;

            pictureBoxDown.Visible = false;
            pictureBoxUp.Visible = true;
        }

        public void CheckRightToLeft()
        {
            if (!StateHolderReader.IsInitialized || DesignMode) return;

            if (TeleoptiPrincipal.Current.Regional.UICulture.TextInfo.IsRightToLeft)
            {
                pictureBoxUp.Location = pictureBox1.Location;
                pictureBoxUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                pictureBox1.Location = pictureBoxDown.Location;
                pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                pictureBoxDown.Location = pictureBoxUp.Location;
                pictureBoxDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                autoLabelHeaderText.RightToLeft = RightToLeft.Yes;
                autoLabelHeaderText.Location = new Point(pictureBoxDown.Location.X - autoLabelHeaderText.Width - 10,
                                                         pictureBoxDown.Location.Y);

                autoLabelHeaderText.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            }
        }

    	public string HeaderText
        {
            get { return autoLabelHeaderText.Text; }
            set{ autoLabelHeaderText.Text = value; }
        }

        private void pictureBoxUp_Click(object sender, EventArgs e)
        {
            OnHideSettings(EventArgs.Empty);
            ToggleShowHideBoxes();
        }

        private void pictureBoxDown_Click(object sender, EventArgs e)
        {
            OnShowSettings(EventArgs.Empty);
            ToggleShowHideBoxes();
        }

        protected virtual void OnShowSettings(EventArgs e)
        {
        	var handler = ShowSettings;
            if (handler!= null) handler(this, e);
        }

        protected virtual void OnHideSettings(EventArgs e)
        {
        	var handler = HideSettings;
            if (handler != null) handler(this, e);
        }

        public void ToggleShowHideBoxes()
        {
            pictureBoxDown.Visible = !pictureBoxDown.Visible;
            pictureBoxUp.Visible = !pictureBoxUp.Visible;
        }

        public void DisableShowSettings()
        {
            pictureBoxUp.Visible = false;
            pictureBoxDown.Visible = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var reportCode = new ReportNameHelper(ReportFunctionCode);
            HelpHelper.Current.GetHelp((BaseUserControl)Parent.Parent, reportCode, false);
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
    	public string ReportFunctionCode { get; set; }

    	private class ReportNameHelper : IHelpContext
        {
            private readonly string _name;

            public ReportNameHelper(string name)
            {
                _name = name;

            }
            public bool HasHelp
            {
                get { return true; }
            }

            public string HelpId
            {
                get { return _name; }
            }

        }
    }
}
