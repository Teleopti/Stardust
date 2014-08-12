using System;
using System.Collections.Generic;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    /// <summary>
    /// Manager for skill day templates
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-03-10
    /// </remarks>
    public partial class SkillDayTemplates : BaseRibbonFormWithUnitOfWork
    {
        private readonly ISkill _skill;
    	private string _newSkillName = string.Empty;


    	public SkillDayTemplates()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public SkillDayTemplates(ISkill skill) : this()
        {
            SkillRepository skillRepository = new SkillRepository(UnitOfWork);
            _skill = skillRepository.Get(skill.Id.GetValueOrDefault());
            InitializeTabs();
            HandleTabSelectionChanged();
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
			InitializeTabs();
			HandleTabSelectionChanged();
    	}

    	protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = string.Concat(Text, " - ", string.IsNullOrEmpty(_newSkillName) ? _skill.Name : _newSkillName);
        }


        private void InitializeTabs()
        {
            IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
            for (int i = 0; i < weekDays.Count; i++)
            {
                TabPageAdv theTabPage = tabControlAdvWeekDays.TabPages[i];

                SkillIntradayTemplateGridControl templateControl = new SkillIntradayTemplateGridControl(_skill.GetTemplateAt((int)weekDays[i]), _skill.TimeZone, _skill.DefaultResolution, _skill.SkillType);
                templateControl.Create();
                GridToChart gridToChartControl = new GridToChart(templateControl);
                theTabPage.Controls.Add(gridToChartControl);
                gridToChartControl.Dock = DockStyle.Fill;
                theTabPage.Tag = weekDays[i];
                theTabPage.Text = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(weekDays[i]);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            try
            {
                _skill.CheckRestrictions();
				TriggerSkillTemplateToUpdateDate();
                PersistAll();
				Close();
            }
            catch (ValidationException validationException)
            {
                string validationErrorMessage = string.Format(CultureInfo.CurrentCulture, validationException.Message);
                ShowErrorMessage(validationErrorMessage, UserTexts.Resources.ValidationError);
            }
        }

    	private void TriggerSkillTemplateToUpdateDate()
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

    	private void btnBack_Click(object sender, EventArgs e)
        {
            tabControlAdvWeekDays.SelectedTab = TabHandler.TabBack(tabControlAdvWeekDays);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            tabControlAdvWeekDays.SelectedTab = TabHandler.TabForward(tabControlAdvWeekDays);
           ((GridToChart) tabControlAdvWeekDays.SelectedTab.Controls[0]).GridControl.Refresh();
        }

        private void HandleTabSelectionChanged()
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

        private void tabControlAdvWeekDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleTabSelectionChanged();
        }
    }
}
