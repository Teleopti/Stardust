using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportDialogGraphicalPresenter
	{
		private readonly ScheduleReportDialogGraphicalModel _model;
		private readonly IScheduleReportDialogGraphicalView _view;

		public ScheduleReportDialogGraphicalPresenter(IScheduleReportDialogGraphicalView view, ScheduleReportDialogGraphicalModel model)
		{
			_view = view;
			_model = model;
		}

		public void OnRadioButtonTeamCheckedChanged(bool value)
		{
			_model.Team = value;
			_view.EnableSortOptions(value);
			_view.EnableSingleFile(!value);
            _view.EnableShowPublicNote(value);

		}

		public void OnRadioButtonIndividualCheckedChanged(bool value)
		{
			_model.Individual = value;
			_view.EnableSortOptions(!value);
			_view.EnableSingleFile(value);
		}

		public void OnRadioButtonSortOnAgentNameCheckedChanged(bool value)
		{
			_model.SortOnAgentName = value;
		}

		public void OnRadioButtonSortOnStartTimeCheckedChanged(bool value)
		{
			_model.SortOnStartTime = value;
		}

		public void OnRadioButtonSortOnEndTimeCheckedChanged(bool value)
		{
			_model.SortOnEndTime = value;
		}

		public void OnCheckBoxSingleFileCheckedChanged(bool value)
		{
			_model.OneFileForSelected = value;
		}

        public void OnCheckBoxShowPublicNoteCheckedChanged(bool value)
        {
            _model.ShowPublicNote = value;
        }

		public void OnButtonCancelClick()
		{
			_view.UserCancel();			
		}

		public void OnButtonOkClick()
		{
			_view.UserOk();	
		}

		public void OnLoad()
		{
			_view.BackgroundColor = ColorHelper.DialogBackColor();
			_view.UpdateFromModel(_model);
		}
	}
}
