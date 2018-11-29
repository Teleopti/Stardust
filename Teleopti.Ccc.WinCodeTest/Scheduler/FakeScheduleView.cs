using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	public class FakeScheduleView : IScheduleViewBase
	{
		private readonly GridControl _grid = new GridControl();
		private IHandleBusinessRuleResponse _handleBusinessRuleResponse;

		public void ShowErrorMessage(string text, string caption)
		{
		}

		public DialogResult ShowConfirmationMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public DialogResult ShowYesNoMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public void ShowInformationMessage(string text, string caption)
		{
		}

		public DialogResult ShowOkCancelMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public DialogResult ShowWarningMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IEnumerable<IAbsence> bindingList, ISetupDateTimePeriod period,
			TimeZoneInfo timeZoneInfo)
		{
			throw new NotImplementedException();
		}

		public IAddActivityViewModel CreateAddActivityViewModel(IEnumerable<IActivity> activities, IList<IShiftCategory> shiftCategories, DateTimePeriod period,
			TimeZoneInfo timeZoneInfo, IActivity defaultActivity)
		{
			throw new NotImplementedException();
		}

		public IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period,
			TimeZoneInfo timeZoneInfo)
		{
			throw new NotImplementedException();
		}

		public IAddOvertimeViewModel CreateAddOvertimeViewModel(IEnumerable<IActivity> activities, IList<IMultiplicatorDefinitionSet> definitionSets, IActivity defaultActivity,
			DateTimePeriod period, TimeZoneInfo timeZoneInfo)
		{
			throw new NotImplementedException();
		}

		public IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo,
			DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public int ColHeaders { get; }
		public int RowHeaders { get; }

		public void SetCellBackTextAndBackColor(GridQueryCellInfoEventArgs e, DateOnly dateTime, bool backColor, bool textColor,
			IScheduleDay schedulePart)
		{
		}

		public string DayHeaderTooltipText(GridStyleInfo gridStyle, DateOnly currentDate)
		{
			throw new NotImplementedException();
		}

		public bool IsRightToLeft { get; }
		public bool IsOverviewColumnsHidden { get; }

		public FakeScheduleView WithBusinessRuleResponse(IHandleBusinessRuleResponse handleBusinessRuleResponse)
		{
			_handleBusinessRuleResponse = handleBusinessRuleResponse;
			return this;
		}

		public IHandleBusinessRuleResponse HandleBusinessRuleResponse { get { return _handleBusinessRuleResponse; } }
		public void InvalidateSelectedRow(IScheduleDay schedulePart)
		{
		}

		public void OnPasteCompleted()
		{
		}

		public GridControl ViewGrid { get { return _grid; } }
		public IList<IScheduleDay> CurrentColumnSelectedSchedules()
		{
			throw new NotImplementedException();
		}

		public IList<IScheduleDay> SelectedSchedules()
		{
			throw new NotImplementedException();
		}

		public void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period)
		{
		}

		public void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo)
		{
		}

		public ICollection<DateOnly> AllSelectedDates()
		{
			throw new NotImplementedException();
		}

		public ICollection<DateOnly> AllSelectedDates(IEnumerable<IScheduleDay> selectedSchedules)
		{
			throw new NotImplementedException();
		}
	}
}