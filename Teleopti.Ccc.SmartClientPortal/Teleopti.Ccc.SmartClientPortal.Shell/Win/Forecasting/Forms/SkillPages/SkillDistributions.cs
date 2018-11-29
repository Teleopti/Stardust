using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
    public partial class SkillDistributions : BaseUserControl, IPropertyPage
    {
        public SkillDistributions()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            moreOrLessButtonOptions.MoreText = UserTexts.Resources.Advanced;
        }

        private void moreOrLessButtonOptions_StateChanged(object sender, EventArgs e)
        {
            panelMoreOptions.Visible = ((MoreOrLessButton)sender).StateAsBoolean;
        }

        private void SetChildSkillVisible()
        {
            panelMoreOptions.Visible = true;
            moreOrLessButtonOptions.Visible = false;
            integerTextBoxServiceLevelSeconds.Visible = false;
            serviceLevelPercentTextBox.Visible = false;
            shrinkagePercentTextBox.Visible = false;
            efficiencyPercentTextBox.Visible = false;
            minimumOccupancyPercentTextBox.Visible = false;
            maximunOccupancyPercentTextBox.Visible = false;
            labelMaximumOccupancy.Visible = false;
            labelMinimumOccupancy.Visible = false;
            labelServiceLevelPercentage.Visible = false;
            labelServiceLevelTarget.Visible = false;
            labelShrinkage.Visible = false;
            labelEfficiency.Visible = false;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as ISkill;
            var skillDayTemplate = (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, DayOfWeek.Monday);
            if (skillDayTemplate.TemplateSkillDataPeriodCollection.Count > 0)
            {
                ITemplateSkillDataPeriod skillDataPeriodTemplate = skillDayTemplate.TemplateSkillDataPeriodCollection[0];

                serviceLevelPercentTextBox.DoubleValue =
                    skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Percent.Value;
                serviceLevelPercentTextBox.DefaultValue = skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Percent.Value * 100;
                integerTextBoxServiceLevelSeconds.IntegerValue =
                    Convert.ToInt32(skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Seconds);
                minimumOccupancyPercentTextBox.DoubleValue = skillDataPeriodTemplate.ServiceAgreement.MinOccupancy.Value;
                maximunOccupancyPercentTextBox.DoubleValue = skillDataPeriodTemplate.ServiceAgreement.MaxOccupancy.Value;
                integerTextBoxMinimumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MinimumPersons;
                integerTextBoxMaximumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MaximumPersons;
                shrinkagePercentTextBox.DoubleValue = skillDataPeriodTemplate.Shrinkage.Value;
                efficiencyPercentTextBox.DoubleValue = skillDataPeriodTemplate.Efficiency.Value;
            }
            if (skill is IChildSkill)
            {
                SetChildSkillVisible();
            }
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as ISkill;

            Percent percent;
            if (!Percent.TryParse(serviceLevelPercentTextBox.Text, out percent)) return false;

            //service level should have a value between 1 and 100
            if (percent.Value <= 0.0 || percent.Value > 1.0) return false;

            var serviceLevel = new ServiceLevel(percent,
                                                         Convert.ToDouble(
                                                             integerTextBoxServiceLevelSeconds.IntegerValue,
                                                             CultureInfo.CurrentCulture));
	        var serviceAgreement = new ServiceAgreement(serviceLevel,
		        new Percent(minimumOccupancyPercentTextBox.DoubleValue),
		        new Percent(maximunOccupancyPercentTextBox.DoubleValue));
        
            var staffMin = (int) integerTextBoxMinimumAgents.IntegerValue;
            var staffMax = (int) integerTextBoxMaximumAgents.IntegerValue;

            var skillPersonData = new SkillPersonData(staffMin, staffMax);

            var shrinkage = new Percent(shrinkagePercentTextBox.DoubleValue);
            var efficiency = new Percent(efficiencyPercentTextBox.DoubleValue);

            DateTime startDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);
            startDateTime = startDateTime.Add(skill.MidnightBreakOffset);

            DateTime endDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.AddDays(1).Date, skill.TimeZone);
            endDateTime = endDateTime.Add(skill.MidnightBreakOffset);
            var timePeriod = new DateTimePeriod(startDateTime, endDateTime);

	        var templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement, skillPersonData, timePeriod)
	        {
		        Shrinkage = shrinkage,
		        Efficiency = efficiency
	        };
	        
	        foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
            {
                var skillDayTemplate =
                    (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, dayOfWeek);
                
                skillDayTemplate.SetSkillDataPeriodCollection(new List<ITemplateSkillDataPeriod>
                                                                  {
                                                                      (ITemplateSkillDataPeriod)templateSkillDataPeriod.Clone()
                                                                  });
                skill.SetTemplateAt((int) dayOfWeek, skillDayTemplate);
            }

            return validateValues(skill);
        }

        private static bool validateValues(ISkill skill)
        {
            try
            {
                skill.CheckRestrictions();
            }
            catch (ValidationException validationException)
            {
                string validationErrorMessage = string.Format(CultureInfo.CurrentCulture, validationException.Message);
                ViewBase.ShowErrorMessage(validationErrorMessage, UserTexts.Resources.ValidationError);
                return false;
            }
            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.Templates; }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }
		
		private void panelMain_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{

		}

		private void labelServiceLevelPercentage_Click(object sender, EventArgs e)
		{

		}

		private void labelServiceLevelTarget_Click(object sender, EventArgs e)
		{

		}

		private void labelMinimumOccupancy_Click(object sender, EventArgs e)
		{

		}

		private void labelMaximumOccupancy_Click(object sender, EventArgs e)
		{

		}

		private void labelShrinkage_Click(object sender, EventArgs e)
		{

		}
    }
}