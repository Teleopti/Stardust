using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
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
			gradientPanel1.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			autoLabelHeaderText.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		public void CheckRightToLeft()
		{
			if (!StateHolderReader.IsInitialized || DesignMode) return;
		}

		public string HeaderText
		{
			get { return autoLabelHeaderText.Text; }
			set{ autoLabelHeaderText.Text = value; }
		}

		public void ShowSpinningProgress(bool show)
		{
			spinningProgressControl1.Visible = show;
		}

		private void pictureBoxUpClick(object sender, EventArgs e)
		{
			OnHideSettings(EventArgs.Empty);
			ToggleShowHideBoxes();
		}

		private void pictureBoxDownClick(object sender, EventArgs e)
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

		private void pictureBox1Click(object sender, EventArgs e)
		{
			var reportCode = new reportNameHelper(ReportFunctionCode);
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

		private class reportNameHelper : IHelpContext
		{
			private readonly string _name;

			public reportNameHelper(string name)
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
