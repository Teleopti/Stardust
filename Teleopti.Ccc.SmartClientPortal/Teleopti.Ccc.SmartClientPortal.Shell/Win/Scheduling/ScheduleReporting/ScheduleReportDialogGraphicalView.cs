using System;
using System.Drawing;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
	public partial class ScheduleReportDialogGraphicalView : BaseDialogForm, IScheduleReportDialogGraphicalView
	{
		private readonly ScheduleReportDialogGraphicalPresenter _presenter;

		public ScheduleReportDialogGraphicalView(ScheduleReportDialogGraphicalModel model)
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			DialogResult = System.Windows.Forms.DialogResult.Cancel;

			_presenter = new ScheduleReportDialogGraphicalPresenter(this, model);
		}

		private void radioButtonTeamCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonTeamCheckedChanged(radioButtonTeam.Checked);
		}

		private void radioButtonIndividualCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonIndividualCheckedChanged(radioButtonIndividual.Checked);
		}

		private void checkBoxSingleFileCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnCheckBoxSingleFileCheckedChanged(checkBoxSingleFile.Checked);
		}

		private void checkBoxPublicNoteCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnCheckBoxShowPublicNoteCheckedChanged(checkBoxPublicNote.Checked);
		}

		private void radioButtonAgentNameCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnAgentNameCheckedChanged(radioButtonAgentName.Checked);
		}

		private void radioButtonStartTimeCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnStartTimeCheckedChanged(radioButtonStartTime.Checked);
		}

		private void radioButtonEndTimeCheckedChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonSortOnEndTimeCheckedChanged(radioButtonEndTime.Checked);
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_presenter.OnButtonOkClick();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_presenter.OnButtonCancelClick();
		}

		private void scheduleReportDialogGraphicalViewLoad(object sender, EventArgs e)
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

		public void EnableShowPublicNote(bool enabled)
		{
			checkBoxPublicNote.Enabled = enabled;
		}

		public void UpdateFromModel(ScheduleReportDialogGraphicalModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			radioButtonTeam.Checked = model.Team;
			radioButtonIndividual.Checked = model.Individual;
			checkBoxSingleFile.Checked = model.OneFileForSelected;
			checkBoxPublicNote.Checked = model.ShowPublicNote;

			radioButtonAgentName.Checked = model.SortOnAgentName;
			radioButtonStartTime.Checked = model.SortOnStartTime;
			radioButtonEndTime.Checked = model.SortOnEndTime;

			EnableSortOptions(model.Team);
			EnableSingleFile(model.Individual);
			EnableShowPublicNote(model.Team);
		}
	}
}
