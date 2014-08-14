using System;
using System.Collections.Generic;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
    public partial class MultisiteDayTemplates : BaseRibbonFormWithUnitOfWork
    {
        private readonly IMultisiteSkill _multisiteSkill;

        public MultisiteDayTemplates()
        {
            InitializeComponent();
            SetColor();
            if (!DesignMode) SetTexts();
        }

        public MultisiteDayTemplates(IMultisiteSkill multisiteSkill)
            : this()
        {
            MultisiteSkillRepository skillRepository = new MultisiteSkillRepository(UnitOfWork);
            _multisiteSkill = skillRepository.Load(multisiteSkill.Id.Value);
            InitializeTabs();
            HandleTabSelectionChanged();
        }

        private void SetColor()
        {
            tabControlAdvWeekDays.SelectedTab.TabForeColor = ColorHelper.TabForegroundColor();
            BrushInfo panelbrush = ColorHelper.ControlGradientPanelBrush();
            gradientPanel1.BackgroundColor = panelbrush;
        }

        private void InitializeTabs()
        {
            IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
            for (int i = 0; i < weekDays.Count; i++)
            {
                TabPageAdv theTabPage = tabControlAdvWeekDays.TabPages[i];

                MultisiteIntradayTemplateGridControl templateControl = new MultisiteIntradayTemplateGridControl(
                    _multisiteSkill, (MultisiteDayTemplate) _multisiteSkill.GetTemplate(TemplateTarget.Multisite, weekDays[i]), 
                    _multisiteSkill.TimeZone, _multisiteSkill.DefaultResolution);
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
                _multisiteSkill.CheckRestrictions();
            	TriggerMultisiteSkillTemplateToUpdateDate();
                PersistAll();
                Close();
            }
            catch (ValidationException validationException)
            {
                string validationErrorMessage = string.Format(CultureInfo.CurrentCulture, validationException.Message);
                ShowErrorMessage(validationErrorMessage, UserTexts.Resources.ValidationError);
            }
        }

		private void TriggerMultisiteSkillTemplateToUpdateDate()
		{
			if (UnitOfWork.IsDirty())
			{
				var weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
				foreach (var t in weekDays)
				{
					var multisiteSkillTemplate = _multisiteSkill.GetTemplate(TemplateTarget.Multisite, t);
					multisiteSkillTemplate.RefreshUpdatedDate();
				}
			}
		}

        private void btnBack_Click(object sender, EventArgs e)
        {
            tabControlAdvWeekDays.SelectedTab = TabHandler.TabBack(tabControlAdvWeekDays);
            RefreshGrid();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            tabControlAdvWeekDays.SelectedTab = TabHandler.TabForward(tabControlAdvWeekDays);
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            ((GridToChart)tabControlAdvWeekDays.SelectedTab.Controls[0]).GridControl.Refresh();
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
            btnBack.Enabled = (tabControlAdvWeekDays.SelectedTab.TabIndex != 1);
            RefreshGrid();
        }

        private void tabControlAdvWeekDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleTabSelectionChanged();
        }

		private void ribbonControlAdv1_Click(object sender, EventArgs e)
		{

		}
    }
}