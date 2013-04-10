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
			IShiftCategory currentShiftCategory = null;
			IAbsence currentAbsence = null;
			IDayOffTemplate currentDayOffTemplate = null;
			IActivity currentActivity = null;

			if (preferenceRestriction != null)
			{
				if (preferenceRestriction.ShiftCategory != null)
				{
					currentShiftCategory = preferenceRestriction.ShiftCategory;
				}

				if (preferenceRestriction.Absence != null)
				{
					currentAbsence = preferenceRestriction.Absence;
				}

				if (preferenceRestriction.DayOffTemplate != null)
				{
					currentDayOffTemplate = preferenceRestriction.DayOffTemplate;
				}

				if (preferenceRestriction.ActivityRestrictionCollection.FirstOrDefault() != null)
				{
					var activityRestriction = preferenceRestriction.ActivityRestrictionCollection[0];
					currentActivity = activityRestriction.Activity;
				}
			}

			populateShiftCategories(currentShiftCategory);
			populateAbsences(currentAbsence);
			populateDayOffs(currentDayOffTemplate);
			populateActivities(currentActivity);
		}

		private void agentPreferenceViewLoad(object sender, EventArgs e)
		{
			_shiftCategories.Insert(0, new ShiftCategory(Resources.None));
			_absences.Insert(0, new Absence(){Description = new Description(Resources.None), Requestable = true});

			_presenter.UpdateView();
		}

		private void populateShiftCategories(IShiftCategory current)
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

		private void populateAbsences(IAbsence current)
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

		private void populateDayOffs(IDayOffTemplate current)
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

		private void populateActivities(IActivity current)
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
