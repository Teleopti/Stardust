using System;
using System.Globalization;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    public partial class SkillEmailDistributions : BaseUserControl, IPropertyPage
    {
        private bool _inputContainsError;
       
        public SkillEmailDistributions()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        private void SetChildSkillVisible()
        {
            timeSpanTextBoxServiceLevelTime.Visible = false;
            percentTextBox1.Visible = false;
            labelShrinkage.Visible = false;
            labelEfficiencyPercentage.Visible = false;
            efficiencyPercentTextBox1.Visible = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void timeSpanTextBoxServiceLevelTime_TimeSpanBoxTextChanged(object sender, EventArgs e)
        {
            if (timeSpanTextBoxServiceLevelTime.ValidatedStatus == ValidatedStatus.Error)
            {
                _inputContainsError = true;
                return;
            }

            _inputContainsError = false;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            ISkill skill = aggregateRoot as ISkill;
            ISkillDayTemplate skillDayTemplate = (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, DayOfWeek.Monday);
            if (skillDayTemplate.TemplateSkillDataPeriodCollection.Count > 0)
            {
                ITemplateSkillDataPeriod skillDataPeriodTemplate = skillDayTemplate.TemplateSkillDataPeriodCollection[0];
                timeSpanTextBoxServiceLevelTime.MaximumValue = TimeSpan.FromSeconds(skill.DefaultResolution * TimeDefinition.SecondsPerMinute * TimeDefinition.HoursPerDay); 
                timeSpanTextBoxServiceLevelTime.Value = (TimeSpan.FromSeconds(skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Seconds));
                integerTextBoxMinimumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MinimumPersons;
                integerTextBoxMaximumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MaximumPersons;
                percentTextBox1.DoubleValue = skillDataPeriodTemplate.Shrinkage.Value;
                efficiencyPercentTextBox1.DoubleValue = skillDataPeriodTemplate.Efficiency.Value;
            }
            if (skill is IChildSkill)
            {
                SetChildSkillVisible();
            }
            
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            if (_inputContainsError) return false;

            ISkill skill = aggregateRoot as ISkill;

            ServiceLevel serviceLevel = new ServiceLevel(new Percent(1d),
                                                         Convert.ToDouble(
                                                             timeSpanTextBoxServiceLevelTime.Value.TotalSeconds,
                                                             CultureInfo.CurrentCulture));
            ServiceAgreement serviceAgreement = new ServiceAgreement();
            serviceAgreement.ServiceLevel = serviceLevel;

            int staffMin = (int) integerTextBoxMinimumAgents.IntegerValue;
            int staffMax = (int) integerTextBoxMaximumAgents.IntegerValue;

            SkillPersonData skillPersonData = new SkillPersonData(staffMin, staffMax);

            Percent shrinkage = new Percent(percentTextBox1.DoubleValue);
            Percent efficiency = new Percent(efficiencyPercentTextBox1.DoubleValue);

            DateTime startDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate, skill.TimeZone);
            startDateTime = startDateTime.Add(skill.MidnightBreakOffset);

            DateTime endDateTime = TimeZoneInfo.ConvertTimeToUtc(
                SkillDayTemplate.BaseDate.AddDays(1), skill.TimeZone);
            endDateTime = endDateTime.Add(skill.MidnightBreakOffset);
            DateTimePeriod timePeriod = new DateTimePeriod(startDateTime, endDateTime);

            ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement,
                                                                                          skillPersonData, timePeriod);
            templateSkillDataPeriod.Shrinkage = shrinkage;
            templateSkillDataPeriod.Efficiency = efficiency;

            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            skillDataPeriods.Add((ITemplateSkillDataPeriod) templateSkillDataPeriod.Clone());

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
            {
                ISkillDayTemplate skillDayTemplate =
                    (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, dayOfWeek);

                skillDayTemplate.SetSkillDataPeriodCollection(new List<ITemplateSkillDataPeriod>
                                                                  {
                                                                      (ITemplateSkillDataPeriod)
                                                                      templateSkillDataPeriod.Clone()
                                                                  });
                skill.SetTemplateAt((int) dayOfWeek, skillDayTemplate);
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
    }
}