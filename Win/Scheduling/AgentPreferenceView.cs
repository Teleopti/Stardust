﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
			if (shiftCategory == null) return;

			ComboBoxAdvShiftCategory currentCategory = null;

			foreach (var item in comboBoxAdvShiftCategory.Items)
			{
				var comboItem  = item as ComboBoxAdvShiftCategory;
				if (comboItem == null || comboItem.Id != shiftCategory.Id) continue;
				currentCategory = comboItem;
				break;
			}

			if (currentCategory != null)
			{
				comboBoxAdvShiftCategory.SelectedItem = currentCategory;
			}
			
		}

		public void UpdateShiftCategoryExtended(IShiftCategory shiftCategory)
		{
			if (shiftCategory == null) return;

			ComboBoxAdvShiftCategory currentCategory = null;

			foreach (var item in comboBoxAdvShiftCategoryExtended.Items)
			{
				var comboItem = item as ComboBoxAdvShiftCategory;
				if (comboItem == null || comboItem.Id != shiftCategory.Id) continue;
				currentCategory = comboItem;
				break;
			}

			if (currentCategory != null)
			{
				comboBoxAdvShiftCategoryExtended.SelectedItem = currentCategory;
			}
		}

		public void UpdateAbsence(IAbsence absence)
		{
			if (absence == null) return;

			Absence currentAbsence = null;

			foreach (var item in comboBoxAdvAbsence.Items)
			{
				var comboItem = item as Absence;
				if (comboItem == null || comboItem.Id != absence.Id) continue;
				currentAbsence = comboItem;
				break;
			}

			if (currentAbsence != null)
			{
				comboBoxAdvAbsence.SelectedItem = currentAbsence;
			}
		}

		public void UpdateDayOff(IDayOffTemplate dayOffTemplate)
		{
			if (dayOffTemplate == null) return;

			ComboBoxAdvDayOffTemplate currentDayOff = null;

			foreach (var item in comboBoxAdvDayOff.Items)
			{
				var comboItem = item as ComboBoxAdvDayOffTemplate;
				if (comboItem == null ||  comboItem.Id != dayOffTemplate.Id) continue;
				currentDayOff = comboItem;
				break;
			}

			if (currentDayOff != null)
			{
				comboBoxAdvDayOff.SelectedItem = currentDayOff;
			}
		}

		public void UpdateActivity(IActivity activity)
		{
			if (activity == null) return;

			Activity currentActivity = null;

			foreach (var item in comboBoxAdvActivity.Items)
			{
				var comboItem = item as Activity;
				if (comboItem == null ||  comboItem.Id != activity.Id) continue;
				currentActivity = comboItem;
				break;
			}

			if (currentActivity != null)
			{
				comboBoxAdvActivity.SelectedItem = currentActivity;
				if (currentActivity.Id != null)
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
			var noneAbsence = new Absence { Description = new Description(Resources.None) };
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

			clearTimeErrorMessagesExtended();
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
			clearTimeErrorMessagesActivity();
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
			var data = viewData();

			var commandToExecute = _presenter.CommandToExecute(data, _dayCreator);

			if (commandToExecute == AgentPreferenceExecuteCommand.Remove)
			{
				var removeCommand = new AgentPreferenceRemoveCommand(_presenter.ScheduleDay);
				_presenter.Remove(removeCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (!validateTimes(data)) return;

			if (commandToExecute == AgentPreferenceExecuteCommand.Add)
			{
				var addCommand = new AgentPreferenceAddCommand(_presenter.ScheduleDay, data, _dayCreator);
				_presenter.Add(addCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (commandToExecute == AgentPreferenceExecuteCommand.Edit)
			{
				var editCommand = new AgentPreferenceEditCommand(_presenter.ScheduleDay, data, _dayCreator);
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

		private bool validateTimes(IAgentPreferenceData data)
		{

			clearTimeErrorMessagesActivity();
			clearTimeErrorMessagesExtended();

			var result = _dayCreator.CanCreate(data);

			if (!result.Result && !result.EmptyError)
			{
				if (result.StartTimeMinError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMin, Resources.MustSpecifyValidTime);
				if (result.StartTimeMaxError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMax, Resources.MustSpecifyValidTime);
				if (result.LengthMinError) setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMin, Resources.MustSpecifyValidTime);
				if (result.LengthMaxError) setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMax, Resources.MustSpecifyValidTime);
				if (result.EndTimeMinError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMin, Resources.MustSpecifyValidTime);
				if (result.EndTimeMaxError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMax, Resources.MustSpecifyValidTime);

				if (result.StartTimeMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityStartMin, Resources.MustSpecifyValidTime);
				if (result.StartTimeMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityStartMax, Resources.MustSpecifyValidTime);
				if (result.LengthMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityLengthMin, Resources.MustSpecifyValidTime);
				if (result.LengthMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityLengthMax, Resources.MustSpecifyValidTime);
				if (result.EndTimeMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityEndMin, Resources.MustSpecifyValidTime);
				if (result.EndTimeMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityEndMax, Resources.MustSpecifyValidTime);
			}

			return result.Result;
		}

		private void clearTimeErrorMessagesActivity()
		{
			setTimeErrorMessageActivity(outlookTimePickerActivityLengthMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityLengthMax, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityStartMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityStartMax, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityEndMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityEndMax, null);
		}

		private void clearTimeErrorMessagesExtended()
		{
			setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMin,null);
			setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMax,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMin,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMax,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMin,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMax, null);
		}

		private bool timesActivityError()
		{
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityLengthMin))) return true;
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityLengthMax))) return true;
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityStartMin))) return true;
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityStartMax))) return true;
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityEndMin))) return true;
			if (string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityEndMax))) return true;

			return false;
		}

		private bool timesExtendedError()
		{
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerContractShiftCategoryMin))) return true;
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerContractShiftCategoryMax))) return true;
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryStartMin))) return true;
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryStartMax))) return true;
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryEndMin))) return true;
			if (string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryEndMax))) return true;

			return false;
		}

		private void setTimeErrorMessageExtended(Control timeControl, string value)
		{
			errorProviderExtended.SetIconPadding(timeControl, 2);
			errorProviderExtended.SetError(timeControl, value);
		}

		private void setTimeErrorMessageActivity(Control timeControl, string value)
		{
			errorProviderActivity.SetIconPadding(timeControl, 2);
			errorProviderActivity.SetError(timeControl, value);
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
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();	
		}

		private void outlookTimePickerContractShiftCategoryMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void checkBoxAdvShiftCategoryNextDayMinCheckStateChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
		}

		private void checkBoxAdvShiftCategoryNextDayMaxCheckStateChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
		}

		private AgentPreferenceData viewData()
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
			var currentShiftCategory = comboShiftCategory.ShiftCategory;
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
			var currentDayOff = comboDayOff.DayOffTemplate;
			if (currentDayOff.Id == null)
			{
				currentDayOff = null;
			}

			var currentActivity = comboBoxAdvActivity.SelectedItem as Activity;
			if (currentActivity.Id == null)
			{
				currentActivity = null;
			}

			var data = new AgentPreferenceData
				{
					ShiftCategory = currentShiftCategory,
					Absence = currentAbsence,
					DayOffTemplate = currentDayOff,
					Activity = currentActivity,
					MinStart = minStart,
					MaxStart = maxStart,
					MinEnd = minEnd,
					MaxEnd = maxEnd,
					MinLength = minLength,
					MaxLength = maxLength,
					MinStartActivity = minStartActivity,
					MaxStartActivity = maxStartActivity,
					MinEndActivity = minEndActivity,
					MaxEndActivity = maxEndActivity,
					MinLengthActivity = minLengthActivity,
					MaxLengthActivity = maxLengthActivity
				};

			return data;
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
