using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class MultisiteDayTemplates : BaseRibbonFormWithUnitOfWork
	{
		private readonly IMultisiteSkill _multisiteSkill;

		public MultisiteDayTemplates()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		public MultisiteDayTemplates(IMultisiteSkill multisiteSkill)
			: this()
		{
			var skillRepository = MultisiteSkillRepository.DONT_USE_CTOR(UnitOfWork);
			_multisiteSkill = skillRepository.Load(multisiteSkill.Id.Value);
			initializeTabs();
			handleTabSelectionChanged();
		}

		private void initializeTabs()
		{
			IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
			for (int i = 0; i < weekDays.Count; i++)
			{
				TabPageAdv theTabPage = tabControlAdvWeekDays.TabPages[i];

				var templateControl = new MultisiteIntradayTemplateGridControl(
					_multisiteSkill, (MultisiteDayTemplate) _multisiteSkill.GetTemplate(TemplateTarget.Multisite, weekDays[i]), 
					_multisiteSkill.TimeZone, _multisiteSkill.DefaultResolution);
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
				_multisiteSkill.CheckRestrictions();
				triggerMultisiteSkillTemplateToUpdateDate();
				PersistAll();
				Close();
			}
			catch (ValidationException validationException)
			{
				string validationErrorMessage = string.Format(CultureInfo.CurrentCulture, validationException.Message);
				ShowErrorMessage(validationErrorMessage, UserTexts.Resources.ValidationError);
			}
		}

		private void triggerMultisiteSkillTemplateToUpdateDate()
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

		private void btnBackClick(object sender, EventArgs e)
		{
			tabControlAdvWeekDays.SelectedTab = TabHandler.TabBack(tabControlAdvWeekDays);
			refreshGrid();
		}

		private void btnForwardClick(object sender, EventArgs e)
		{
			tabControlAdvWeekDays.SelectedTab = TabHandler.TabForward(tabControlAdvWeekDays);
			refreshGrid();
		}

		private void refreshGrid()
		{
			((GridToChart)tabControlAdvWeekDays.SelectedTab.Controls[0]).GridControl.Refresh();
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
			btnBack.Enabled = (tabControlAdvWeekDays.SelectedTab.TabIndex != 1);
			refreshGrid();
		}

		private void tabControlAdvWeekDaysSelectedIndexChanged(object sender, EventArgs e)
		{
			handleTabSelectionChanged();
		}
	}
}