using System;
using System.Drawing;
using System.Threading;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
	public partial class ScheduleReportDialogGraphicalView : BaseRibbonForm, IScheduleReportDialogGraphicalView
	{
		private readonly ScheduleReportDialogGraphicalPresenter _presenter;

		public ScheduleReportDialogGraphicalView(ScheduleReportDialogGraphicalModel model)
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			DialogResult = System.Windows.Forms.DialogResult.Cancel;

			_presenter = new ScheduleReportDialogGraphicalPresenter(this, model);
		}

		private void RadioButtonTeamCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonTeamCheckedChanged(radioButtonTeam.Checked);
		}

		private void RadioButtonIndividualCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonIndividualCheckedChanged(radioButtonIndividual.Checked);
		}

		private void CheckBoxSingleFileCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnCheckBoxSingleFileCheckedChanged(checkBoxSingleFile.Checked);
		}

		private void RadioButtonAgentNameCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnAgentNameCheckedChanged(radioButtonAgentName.Checked);
		}

		private void RadioButtonStartTimeCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnStartTimeCheckedChanged(radioButtonStartTime.Checked);
		}

		private void RadioButtonEndTimeCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnEndTimeCheckedChanged(radioButtonEndTime.Checked);
		}

		private void ButtonAdvOkClick(object sender, EventArgs e)
		{
			_presenter.OnButtonOkClick();
		}

		private void ButtonAdvCancelClick(object sender, EventArgs e)
		{
			_presenter.OnButtonCancelClick();
		}

		private void ScheduleReportDialogGraphicalViewLoad(object sender, EventArgs e)
		{
			_presenter.OnLoad();
		}

		public void UserCancel()
		{
			Close();
		}

		public void UserOk()
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		public Color BackgroundColor
		{
			get { return BackColor; }
			set { BackColor = value; }
		}

		public void EnableSortOptions(bool enabled)
		{
			groupBox2.Enabled = enabled;
		}

		public void EnableSingleFile(bool enabled)
		{
			checkBoxSingleFile.Enabled = enabled;
		}

		public void UpdateFromModel(ScheduleReportDialogGraphicalModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			radioButtonTeam.Checked = model.Team;
			radioButtonIndividual.Checked = model.Individual;
			checkBoxSingleFile.Checked = model.OneFileForSelected;

			radioButtonAgentName.Checked = model.SortOnAgentName;
			radioButtonStartTime.Checked = model.SortOnStartTime;
			radioButtonEndTime.Checked = model.SortOnEndTime;

			EnableSortOptions(model.Team);
			EnableSingleFile(model.Individual);
		}
	}
}
