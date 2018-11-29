using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{

	public partial class SkillDayTemplates : BaseRibbonFormWithUnitOfWork
	{
		private readonly ISkill _skill;
		private readonly string _newSkillName = string.Empty;


		public SkillDayTemplates()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		public SkillDayTemplates(ISkill skill) : this()
		{
			var skillRepository = new SkillRepository(UnitOfWork);
			_skill = skillRepository.Get(skill.Id.GetValueOrDefault());
			initializeTabs();
			handleTabSelectionChanged();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public SkillDayTemplates(ISkill skill, bool reloadSkill) : this()
		{
			if (reloadSkill)
			{
				var skillRepository = new SkillRepository(UnitOfWork);
				_skill = skillRepository.Get(skill.Id.GetValueOrDefault());
			}
			else
				_skill = skill;
			initializeTabs();
			handleTabSelectionChanged();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Text = string.Concat(Text, " - ", string.IsNullOrEmpty(_newSkillName) ? _skill.Name : _newSkillName);
		}


		private void initializeTabs()
		{
			IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
			for (int i = 0; i < weekDays.Count; i++)
			{
				TabPageAdv theTabPage = tabControlAdvWeekDays.TabPages[i];

				var templateControl = new SkillIntradayTemplateGridControl(_skill.GetTemplateAt((int)weekDays[i]), _skill.TimeZone, _skill.DefaultResolution, _skill.SkillType);
				templateControl.Create();
				var gridToChartControl = new GridToChart(templateControl);
				theTabPage.Controls.Add(gridToChartControl);
				gridToChartControl.Dock = DockStyle.Fill;
				theTabPage.Tag = weekDays[i];
				theTabPage.Text = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(weekDays[i]);
			}
		}

		private void btnCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		private void btnFinishClick(object sender, EventArgs e)
		{
			try
			{
				_skill.CheckRestrictions();
				triggerSkillTemplateToUpdateDate();
				PersistAll();
				Close();
			}
			catch (ValidationException validationException)
			{
				string validationErrorMessage = string.Format(CultureInfo.CurrentCulture, validationException.Message);
				ShowErrorMessage(validationErrorMessage, UserTexts.Resources.ValidationError);
			}
		}

		private void triggerSkillTemplateToUpdateDate()
		{
			if (UnitOfWork.IsDirty())
			{
				var weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
				foreach (DayOfWeek t in weekDays)
				{
					var skillTemplate = _skill.GetTemplateAt((int)t);
					skillTemplate.RefreshUpdatedDate();
				}
			}   	
		}

		private void btnBackClick(object sender, EventArgs e)
		{
			tabControlAdvWeekDays.SelectedTab = TabHandler.TabBack(tabControlAdvWeekDays);
		}

		private void btnForwardClick(object sender, EventArgs e)
		{
			tabControlAdvWeekDays.SelectedTab = TabHandler.TabForward(tabControlAdvWeekDays);
		   ((GridToChart) tabControlAdvWeekDays.SelectedTab.Controls[0]).GridControl.Refresh();
		}

		private void handleTabSelectionChanged()
		{
			if (tabControlAdvWeekDays.SelectedTab.TabIndex == tabControlAdvWeekDays.TabCount)
			{
				btnForward.Enabled = false;
				AcceptButton = btnFinish;
			}
			else
			{
				btnForward.Enabled = true;
				AcceptButton = btnForward;
			}
			if (tabControlAdvWeekDays.SelectedTab.TabIndex == 1)
			{
				btnBack.Enabled = false;
			}
			else
			{
				btnBack.Enabled = true;
			}
		}

		private void tabControlAdvWeekDaysSelectedIndexChanged(object sender, EventArgs e)
		{
			handleTabSelectionChanged();
		}
	}
}
