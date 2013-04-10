using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	

	public partial class AgentPreferenceView : BaseRibbonForm, IAgentPreferenceView
	{
		private IAgentPreferenceDayCreator _dayCreator;
		private readonly AgentPreferencePresenter _presenter;
		private bool _isDirty;
		private readonly IList<IShiftCategory> _shiftCategories;
		private readonly IList<IAbsence> _absences;
		private readonly IList<IWorkflowControlSet> _workflowControlSets;

		public AgentPreferenceView(IScheduleDay scheduleDay, IList<IShiftCategory> shiftCategories, IList<IAbsence> absences, IList<IWorkflowControlSet> workflowControlSets)
		{
			InitializeComponent();
			SetTexts();
			_dayCreator = new AgentPreferenceDayCreator();
			_presenter = new AgentPreferencePresenter(this, scheduleDay);
			_shiftCategories = shiftCategories;
			_absences = absences;
			_workflowControlSets = workflowControlSets;
		}

		public IScheduleDay ScheduleDay
		{
			get { return _isDirty ? _presenter.ScheduleDay : null; }
		}

		public void Update(IPreferenceRestriction preferenceRestriction)
		{
			if (preferenceRestriction == null)
			{
				ClearShiftCategory();
				ClearShiftCategoryExtended();
				ClearAbsence();
				ClearDayOff();
				ClearActivity();
			}
		}

		public void UpdateShiftCategory(IShiftCategory shiftCategory)
		{
			ComboBoxAdvShiftCategory comboItem = null;

			foreach (var item in comboBoxAdvShiftCategory.Items)
			{
				if (((ComboBoxAdvShiftCategory) item).Id == shiftCategory.Id)
				{
					comboItem = item as ComboBoxAdvShiftCategory;
					break;
				}

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
				if (((ComboBoxAdvShiftCategory)item).Id == shiftCategory.Id)
				{
					comboItem = item as ComboBoxAdvShiftCategory;
					break;
				}

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
				if (((Absence) item).Id == absence.Id)
				{
					comboItem = item as Absence;
					break;
				}
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
				if (((ComboBoxAdvDayOffTemplate)item).Id == dayOffTemplate.Id)
				{
					comboItem = item as ComboBoxAdvDayOffTemplate;
					break;
				}

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
				if (((Activity)item).Id == activity.Id)
				{
					comboItem = item as Activity;
					break;
				}
			}

			if (comboItem != null)
			{
				comboBoxAdvActivity.SelectedItem = comboItem;
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
			outlookTimePickerShiftCategoryEndMin.SetTimeValue(minEnd);
			outlookTimePickerShiftCategoryEndMax.SetTimeValue(maxEnd);
		}

		public void UpdateMustHave(bool mustHave)
		{
			checkBoxMustHaveStandard.Checked = mustHave;
			checkBoxAdvShiftCategoryMustHave.Checked = mustHave;
			checkBoxAdvMustHaveActivity.Checked = mustHave;
		}

		private void agentPreferenceViewLoad(object sender, EventArgs e)
		{
			_presenter.UpdateView();
		}

		public void PopulateShiftCategories()
		{
			var workflowControlSet = _presenter.ScheduleDay.Person.WorkflowControlSet;
			var comboCategories = new List<ComboBoxAdvShiftCategory>();

			foreach (var controlSet in _workflowControlSets)
			{
				if (controlSet.Id == workflowControlSet.Id)
				{
					workflowControlSet = controlSet;
					break;
				}
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
				if (controlSet.Id == workflowControlSet.Id)
				{
					workflowControlSet = controlSet;
					break;
				}
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
				if (controlSet.Id == workflowControlSet.Id)
				{
					workflowControlSet = controlSet;
					break;
				}
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
				if (controlSet.Id == workflowControlSet.Id)
				{
					workflowControlSet = controlSet;
					break;
				}
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
			checkBoxAdvShiftCategoryNextDay.Checked = false;
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
		}

		private void tabControlAgentInfoSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.UpdateView();
		}

	}

	class ComboBoxAdvShiftCategory
	{
		private readonly IShiftCategory _shiftCategory;

		private ComboBoxAdvShiftCategory() { }

		public ComboBoxAdvShiftCategory(IShiftCategory shiftCategory)
			: this()
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
	}
}
