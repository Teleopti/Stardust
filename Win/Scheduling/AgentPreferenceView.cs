using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class AgentPreferenceView : BaseRibbonForm, IAgentPreferenceView
	{
		private readonly IAgentPreferenceDayCreator _dayCreator;
		private readonly AgentPreferencePresenter _presenter;
		private bool _isDirty;
		private readonly IList<IWorkflowControlSet> _workflowControlSets;
		private bool _isInitialized;

		public AgentPreferenceView(IScheduleDay scheduleDay, IList<IWorkflowControlSet> workflowControlSets)
		{
			InitializeComponent();
			SetTexts();
			_dayCreator = new AgentPreferenceDayCreator();
			_presenter = new AgentPreferencePresenter(this, scheduleDay);
			_workflowControlSets = workflowControlSets;
		}

		public IScheduleDay ScheduleDay
		{
			get { return _isDirty ? _presenter.ScheduleDay : null; }
		}

		public void UpdateShiftCategory(IShiftCategory shiftCategory)
		{
			ComboBoxAdvShiftCategory comboItem = null;

			foreach (var item in comboBoxAdvShiftCategory.Items)
			{
				if (((ComboBoxAdvShiftCategory) item).Id != shiftCategory.Id) continue;
				comboItem = item as ComboBoxAdvShiftCategory;
				break;
			}
			
			if (comboItem != null)
			{
				comboBoxAdvShiftCategory.SelectedItem = comboItem;
			}
			
		}

		public void UpdateShiftCategoryExtended(IShiftCategory shiftCategory)
		{
			ComboBoxAdvShiftCategory comboItem = null;

			foreach (var item in comboBoxAdvShiftCategoryExtended.Items)
			{
				if (((ComboBoxAdvShiftCategory) item).Id != shiftCategory.Id) continue;
				comboItem = item as ComboBoxAdvShiftCategory;
				break;
			}

			if (comboItem != null)
			{
				comboBoxAdvShiftCategoryExtended.SelectedItem = comboItem;
			}
		}

		public void UpdateAbsence(IAbsence absence)
		{
			Absence comboItem = null;

			foreach (var item in comboBoxAdvAbsence.Items)
			{
				if (((Absence) item).Id != absence.Id) continue;
				comboItem = item as Absence;
				break;
			}

			if (comboItem != null)
			{
				comboBoxAdvAbsence.SelectedItem = comboItem;
			}
		}

		public void UpdateDayOff(IDayOffTemplate dayOffTemplate)
		{
			ComboBoxAdvDayOffTemplate comboItem = null;

			foreach (var item in comboBoxAdvDayOff.Items)
			{
				if (((ComboBoxAdvDayOffTemplate) item).Id != dayOffTemplate.Id) continue;
				comboItem = item as ComboBoxAdvDayOffTemplate;
				break;
			}

			if (comboItem != null)
			{
				comboBoxAdvDayOff.SelectedItem = comboItem;
			}
		}

		public void UpdateActivity(IActivity activity)
		{
			Activity comboItem = null;

			foreach (var item in comboBoxAdvActivity.Items)
			{
				if (((Activity) item).Id != activity.Id) continue;
				comboItem = item as Activity;
				break;
			}

			if (comboItem != null)
			{
				comboBoxAdvActivity.SelectedItem = comboItem;
				if (comboItem.Id != null)
				{
					EnableActivityTimes(true);
				}
			}
		}

		public void UpdateActivityTimes(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			outlookTimePickerActivityLengthMin.SetTimeValue(minLength);
			outlookTimePickerActivityLengthMax.SetTimeValue(maxLength);
			outlookTimePickerActivityStartMin.SetTimeValue(minStart);
			outlookTimePickerActivityStartMax.SetTimeValue(maxStart);
			outlookTimePickerActivityEndMin.SetTimeValue(minEnd);
			outlookTimePickerActivityEndMax.SetTimeValue(maxEnd);	
		}

		public void UpdateTimesExtended(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			outlookTimePickerContractShiftCategoryMin.SetTimeValue(minLength);
			outlookTimePickerContractShiftCategoryMax.SetTimeValue(maxLength);
			outlookTimePickerShiftCategoryStartMin.SetTimeValue(minStart);
			outlookTimePickerShiftCategoryStartMax.SetTimeValue(maxStart);
			
			if (minEnd.HasValue && minEnd.Value.Days > 0)
			{
				minEnd = minEnd.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvShiftCategoryNextDayMin.Checked = true;
			}
			else
			{
				checkBoxAdvShiftCategoryNextDayMin.Checked = false;
			}
			
			outlookTimePickerShiftCategoryEndMin.SetTimeValue(minEnd);

			if (maxEnd.HasValue && maxEnd.Value.Days > 0)
			{
				maxEnd = maxEnd.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvShiftCategoryNextDayMax.Checked = true;
			}
			else
			{
				checkBoxAdvShiftCategoryNextDayMax.Checked = false;
			}

			outlookTimePickerShiftCategoryEndMax.SetTimeValue(maxEnd);
		}

		public void UpdateMustHave(bool mustHave)
		{
			checkBoxMustHave.Checked = mustHave;
		}

		private void agentPreferenceViewLoad(object sender, EventArgs e)
		{
			tabControlAgentInfo.SelectedIndex = 2;
			tabControlAgentInfo.SelectedIndex = 1;
			tabControlAgentInfo.SelectedIndex = 0;
			_presenter.UpdateView();
			_isInitialized = true;
		}

		public void PopulateShiftCategories()
		{
			var workflowControlSet = _presenter.ScheduleDay.Person.WorkflowControlSet;
			var comboCategories = new List<ComboBoxAdvShiftCategory>();

			foreach (var controlSet in _workflowControlSets)
			{
				if (controlSet.Id != workflowControlSet.Id) continue;
				workflowControlSet = controlSet;
				break;
			}

			foreach (var shiftCategory in workflowControlSet.AllowedPreferenceShiftCategories)
			{
				comboCategories.Add((new ComboBoxAdvShiftCategory(shiftCategory)));	
			}
			
			var sortedCategories = (from c in comboCategories orderby c.Name select c).ToList();
			var noneCategory = new ShiftCategory(Resources.None);
			sortedCategories.Insert(0, new ComboBoxAdvShiftCategory(noneCategory));

			comboBoxAdvShiftCategory.DisplayMember = "Name";
			comboBoxAdvShiftCategory.ValueMember = "Id";
			comboBoxAdvShiftCategory.DataSource = sortedCategories;

			comboBoxAdvShiftCategoryExtended.DisplayMember = "Name";
			comboBoxAdvShiftCategoryExtended.ValueMember = "Id";
			comboBoxAdvShiftCategoryExtended.DataSource = sortedCategories;
		}

		public void PopulateAbsences()
		{
			var workflowControlSet = _presenter.ScheduleDay.Person.WorkflowControlSet;
			var absences = new List<IAbsence>();

			foreach (var controlSet in _workflowControlSets)
			{
				if (controlSet.Id != workflowControlSet.Id) continue;
				workflowControlSet = controlSet;
				break;
			}

			foreach (var absence in workflowControlSet.AllowedPreferenceAbsences)
			{
				absences.Add(absence);
			}

			var sortedAbsences = (from a in absences orderby a.Name select a).ToList();
			var noneAbsence = new Absence() { Description = new Description(Resources.None) };
			sortedAbsences.Insert(0, noneAbsence);

			comboBoxAdvAbsence.DisplayMember = "Name";
			comboBoxAdvAbsence.ValueMember = "Id";
			comboBoxAdvAbsence.DataSource = sortedAbsences;	
		}

		public void PopulateDayOffs()
		{
			var workflowControlSet = _presenter.ScheduleDay.Person.WorkflowControlSet;
			var comboDayOffs = new List<ComboBoxAdvDayOffTemplate>();

			foreach (var controlSet in _workflowControlSets)
			{
				if (controlSet.Id != workflowControlSet.Id) continue;
				workflowControlSet = controlSet;
				break;
			}

			foreach (var dayOff in workflowControlSet.AllowedPreferenceDayOffs)
			{
				comboDayOffs.Add((new ComboBoxAdvDayOffTemplate(dayOff)));
			}

			var sortedDayOffs = (from d in comboDayOffs orderby d.Name select d).ToList();
			var noneDayOff = new DayOffTemplate(new Description(Resources.None));
			sortedDayOffs.Insert(0, new ComboBoxAdvDayOffTemplate(noneDayOff));

			comboBoxAdvDayOff.DisplayMember = "Name";
			comboBoxAdvDayOff.ValueMember = "Id";
			comboBoxAdvDayOff.DataSource = sortedDayOffs;
		
		}

		public void PopulateActivities()
		{
			var workflowControlSet = _presenter.ScheduleDay.Person.WorkflowControlSet;
			var activities = new List<IActivity>();

			foreach (var controlSet in _workflowControlSets)
			{
				if (controlSet.Id != workflowControlSet.Id) continue;
				workflowControlSet = controlSet;
				break;
			}

			if(workflowControlSet.AllowedPreferenceActivity != null)
				activities.Add(workflowControlSet.AllowedPreferenceActivity);

			var noneActivity = new Activity(Resources.None);
			activities.Insert(0, noneActivity);

			comboBoxAdvActivity.DisplayMember = "Name";
			comboBoxAdvActivity.ValueMember = "Id";
			comboBoxAdvActivity.DataSource = activities;
		}

		public void ClearShiftCategory()
		{
			comboBoxAdvShiftCategory.SelectedIndex = 0;
		}

		public void ClearShiftCategoryExtended()
		{
			comboBoxAdvShiftCategoryExtended.SelectedIndex = 0;
			outlookTimePickerContractShiftCategoryMin.SetTimeValue(null);
			outlookTimePickerContractShiftCategoryMax.SetTimeValue(null);
			outlookTimePickerShiftCategoryStartMin.SetTimeValue(null);
			outlookTimePickerShiftCategoryStartMax.SetTimeValue(null);
			outlookTimePickerShiftCategoryEndMin.SetTimeValue(null);
			outlookTimePickerShiftCategoryEndMax.SetTimeValue(null);
			checkBoxAdvShiftCategoryNextDayMax.Checked = false;
			checkBoxAdvShiftCategoryNextDayMin.Checked = false;
		}

		public void ClearAbsence()
		{
			comboBoxAdvAbsence.SelectedIndex = 0;
		}

		public void ClearDayOff()
		{
			comboBoxAdvDayOff.SelectedIndex = 0;
		}

		public void ClearActivity()
		{
			comboBoxAdvActivity.SelectedIndex = 0;
			outlookTimePickerActivityLengthMin.SetTimeValue(null);
			outlookTimePickerActivityLengthMax.SetTimeValue(null);
			outlookTimePickerActivityStartMin.SetTimeValue(null);
			outlookTimePickerActivityStartMax.SetTimeValue(null);
			outlookTimePickerActivityEndMin.SetTimeValue(null);
			outlookTimePickerActivityEndMax.SetTimeValue(null);

			EnableActivityTimes(false);
		}

		public void EnableActivityTimes(bool enable)
		{
			outlookTimePickerActivityLengthMin.Enabled = enable;
			outlookTimePickerActivityLengthMax.Enabled = enable;
			outlookTimePickerActivityStartMin.Enabled = enable;
			outlookTimePickerActivityStartMax.Enabled = enable;
			outlookTimePickerActivityEndMin.Enabled = enable;
			outlookTimePickerActivityEndMax.Enabled = enable;	
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			var minStart = outlookTimePickerShiftCategoryStartMin.TimeValue();
			var maxStart = outlookTimePickerShiftCategoryStartMax.TimeValue();
			var minEnd = outlookTimePickerShiftCategoryEndMin.TimeValue();
			var maxEnd = outlookTimePickerShiftCategoryEndMax.TimeValue();
			var minLength = outlookTimePickerContractShiftCategoryMin.TimeValue();
			var maxLength = outlookTimePickerContractShiftCategoryMax.TimeValue();

			if (checkBoxAdvShiftCategoryNextDayMin.Checked && minEnd != null)
				minEnd = minEnd.Value.Add(TimeSpan.FromDays(1));

			if (checkBoxAdvShiftCategoryNextDayMax.Checked && maxEnd != null)
				maxEnd = maxEnd.Value.Add(TimeSpan.FromDays(1));
			

			var minStartActivity = outlookTimePickerActivityStartMin.TimeValue();
			var maxStartActivity = outlookTimePickerActivityStartMax.TimeValue();
			var minEndActivity = outlookTimePickerActivityEndMin.TimeValue();
			var maxEndActivity = outlookTimePickerActivityEndMax.TimeValue();
			var minLengthActivity = outlookTimePickerActivityLengthMin.TimeValue();
			var maxLengthActivity = outlookTimePickerActivityLengthMax.TimeValue();

			var comboShiftCategory = comboBoxAdvShiftCategory.SelectedItem as ComboBoxAdvShiftCategory;
			IShiftCategory currentShiftCategory = null;
			currentShiftCategory = comboShiftCategory.ShiftCategory;
			if (currentShiftCategory.Id == null)
			{
				currentShiftCategory = null;
			}

			var currentAbsence = comboBoxAdvAbsence.SelectedItem as Absence;
			if (currentAbsence.Id == null)
			{
				currentAbsence = null;
			}

			var comboDayOff = comboBoxAdvDayOff.SelectedItem as ComboBoxAdvDayOffTemplate;
			IDayOffTemplate currentDayOff = null;
			currentDayOff = comboDayOff.DayOffTemplate;
			if (currentDayOff.Id == null)
			{
				currentDayOff = null;
			}

			var currentActivity = comboBoxAdvActivity.SelectedItem as Activity;
			if (currentActivity.Id == null)
			{
				currentActivity = null;
			}

			var commandToExecute = _presenter.CommandToExecute(currentShiftCategory, currentAbsence, currentDayOff, currentActivity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity, _dayCreator);

			if (commandToExecute == AgentPreferenceExecuteCommand.Remove)
			{
				var removeCommand = new AgentPreferenceRemoveCommand(_presenter.ScheduleDay);
				_presenter.Remove(removeCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (!validateTimes(currentShiftCategory, currentAbsence, currentDayOff, currentActivity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity)) return;

			if (commandToExecute == AgentPreferenceExecuteCommand.Add)
			{
				var addCommand = new AgentPreferenceAddCommand(_presenter.ScheduleDay, currentShiftCategory, currentAbsence, currentDayOff, currentActivity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity, _dayCreator);
				_presenter.Add(addCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (commandToExecute == AgentPreferenceExecuteCommand.Edit)
			{
				var editCommand = new AgentPreferenceEditCommand(_presenter.ScheduleDay, currentShiftCategory, currentAbsence, currentDayOff, currentActivity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity, _dayCreator);
				_presenter.Edit(editCommand);
				_isDirty = true;
				Hide();
				return;
			}

			_isDirty = false;
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_isDirty = false;
			Hide();
		}

		private bool validateTimes(IShiftCategory shiftCategory, IAbsence absence, IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart, TimeSpan? maxStart,
									TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
									TimeSpan? minStartActivity, TimeSpan? maxStartActivity, TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
									TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity)
		{


			var result = _dayCreator.CanCreate(shiftCategory, absence, dayOffTemplate, activity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity);

			if (!result.Result && !result.EmptyError)
			{
					
			}

			//if (!result && startTimeError != endTimeError)
			//{
			//	if (startTimeError) setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
			//	if (endTimeError) setTimeErrorMessage(outlookTimePickerTo, Resources.MustSpecifyValidTime);
			//}

			return result.Result;
		}

		private void comboBoxAdvShiftCategorySelectedIndexChanged(object sender, EventArgs e)
		{
			var comboShiftCategory = comboBoxAdvShiftCategory.SelectedItem as ComboBoxAdvShiftCategory;
			if (comboShiftCategory == null || comboShiftCategory.Id == null) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void comboBoxAdvAbsenceSelectedIndexChanged(object sender, EventArgs e)
		{
			var currentAbsence = comboBoxAdvAbsence.SelectedItem as Absence;
			if (currentAbsence == null || currentAbsence.Id == null) return;
			ClearShiftCategory();
			ClearShiftCategoryExtended();
			ClearDayOff();
			ClearActivity();
		}

		private void comboBoxAdvDayOffSelectedIndexChanged(object sender, EventArgs e)
		{
			var comboDayOff = comboBoxAdvDayOff.SelectedItem as ComboBoxAdvDayOffTemplate;
			if (comboDayOff == null || comboDayOff.Id == null) return;
			ClearShiftCategory();
			ClearShiftCategoryExtended();
			ClearAbsence();
			ClearActivity();
		}

		private void comboBoxAdvShiftCategoryExtendedSelectedIndexChanged(object sender, EventArgs e)
		{
			var comboShiftCategory = comboBoxAdvShiftCategoryExtended.SelectedItem as ComboBoxAdvShiftCategory;
			if (comboShiftCategory == null || comboShiftCategory.Id == null) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void comboBoxAdvActivitySelectedIndexChanged(object sender, EventArgs e)
		{
			var comboActivity = comboBoxAdvActivity.SelectedItem as Activity;
			if (comboActivity != null && comboActivity.Id != null)
			{
				ClearAbsence();
				ClearDayOff();
				EnableActivityTimes(true);
			}
			else
			{
				ClearActivity();
			}
		}

		private void outlookTimePickerContractShiftCategoryMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();	
		}

		private void outlookTimePickerContractShiftCategoryMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			ClearAbsence();
			ClearDayOff();
		}
	}

	class ComboBoxAdvShiftCategory
	{
		private readonly IShiftCategory _shiftCategory;

		private ComboBoxAdvShiftCategory() { }

		public ComboBoxAdvShiftCategory(IShiftCategory shiftCategory) : this()
		{
			_shiftCategory = shiftCategory;
		}

		public string Name
		{
			get { return _shiftCategory.Description.Name; }
		}

		public Guid? Id
		{
			get { return _shiftCategory.Id; }
		}

		public IShiftCategory ShiftCategory
		{
			get { return _shiftCategory; }
		}
	}

	class ComboBoxAdvDayOffTemplate
	{
		private readonly IDayOffTemplate _dayOffTemplate;

		private ComboBoxAdvDayOffTemplate() { }

		public ComboBoxAdvDayOffTemplate(IDayOffTemplate dayOffTemplate) : this()
		{
			_dayOffTemplate = dayOffTemplate;
		}

		public string Name
		{
			get { return _dayOffTemplate.Description.Name; }
		}

		public Guid? Id
		{
			get { return _dayOffTemplate.Id; }
		}

		public IDayOffTemplate DayOffTemplate
		{
			get { return _dayOffTemplate; }
		}
	}
}
