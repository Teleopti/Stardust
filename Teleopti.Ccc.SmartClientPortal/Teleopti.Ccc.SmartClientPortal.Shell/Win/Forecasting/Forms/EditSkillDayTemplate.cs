using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class EditSkillDayTemplate : BaseRibbonFormWithUnitOfWork
	{
		private readonly ISkill _skill;
		private ISkillDayTemplate _skillDayTemplate;
		private readonly string _nameEmptyText = UserTexts.Resources.EnterNameHere;
		private readonly ToolTip _toolTip = new ToolTip();

		public EditSkillDayTemplate()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			_toolTip.IsBalloon = true;
			_toolTip.InitialDelay = 1000;
			_toolTip.ToolTipTitle = UserTexts.Resources.InvalidAgentName;
		}

		public EditSkillDayTemplate(ISkillDayTemplate skillDayTemplate) : this()
		{
			var skillRepository = SkillRepository.DONT_USE_CTOR(UnitOfWork);
			_skill = skillRepository.Get(skillDayTemplate.Parent.Id.GetValueOrDefault());
			_skillDayTemplate = (ISkillDayTemplate) _skill.TryFindTemplateByName(TemplateTarget.Skill, skillDayTemplate.Name);
		}

		public EditSkillDayTemplate(IEntity skill) : this()
		{
			var skillRepository = SkillRepository.DONT_USE_CTOR(UnitOfWork);
			_skill = skillRepository.Get(skill.Id.GetValueOrDefault());

			var serviceAgreement = initializeServiceAgreement();
			var timePeriod = initializeTimePeriod();

			createNewSkillDayTemplate(serviceAgreement, timePeriod);
		}

		public EditSkillDayTemplate(ISkillDay skillDay)
			: this()
		{
			if (skillDay == null) throw new ArgumentNullException("skillDay");
			var skillRepository = SkillRepository.DONT_USE_CTOR(UnitOfWork);
			_skill = skillRepository.Get(skillDay.Skill.Id.GetValueOrDefault());
			
			var serviceAgreement = initializeServiceAgreement();
			var timePeriod = initializeTimePeriod();

			createNewSkillDayTemplate(serviceAgreement, timePeriod);
			initializeSkillDayTemplateFromSkillDay(skillDay);
		}

		private DateTimePeriod initializeTimePeriod()
		{
			var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone);
			return new DateTimePeriod(startDateUtc, startDateUtc.AddDays(1)).MovePeriod(_skill.MidnightBreakOffset);
		}

		private ServiceAgreement initializeServiceAgreement()
		{
			var serviceAgreement = _skill.SkillType.ForecastSource != ForecastSource.InboundTelephony && _skill.SkillType.ForecastSource != ForecastSource.Retail ? ServiceAgreement.DefaultValuesEmail() : ServiceAgreement.DefaultValues();
			return serviceAgreement;
		}

		private void createNewSkillDayTemplate(ServiceAgreement serviceAgreement, DateTimePeriod timePeriod)
		{
			ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement, new SkillPersonData(), timePeriod);
			_skillDayTemplate = new SkillDayTemplate("New Template",
													 new List<ITemplateSkillDataPeriod>
														{
															templateSkillDataPeriod
														});
			textBoxTemplateName.ReadOnly = false;
			_skill.AddTemplate(_skillDayTemplate);
		}

		private void initializeSkillDayTemplateFromSkillDay(ISkillDay skillDay)
		{
			var baseDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone);
			var actualDateUtc = TimeZoneInfo.ConvertTimeToUtc(skillDay.CurrentDate.Date, _skill.TimeZone);
			var difference = actualDateUtc.Subtract(baseDateUtc);
			var tempalteSkillDataPeriodCollection = skillDay.SkillDataPeriodCollection.Select(
				skillDataPeriod =>
				new TemplateSkillDataPeriod(skillDataPeriod.ServiceAgreement, skillDataPeriod.SkillPersonData,
											skillDataPeriod.Period.MovePeriod(difference.Negate()))
					{
						Shrinkage = skillDataPeriod.Shrinkage,
						Efficiency = skillDataPeriod.Efficiency,
						ManualAgents = skillDataPeriod.ManualAgents
				}).Cast<ITemplateSkillDataPeriod>().ToList();
			_skillDayTemplate.SetSkillDataPeriodCollection(tempalteSkillDataPeriodCollection);
		}

		private void editTemplateLoad(object sender, EventArgs e)
		{
			var templateControl = new SkillIntradayTemplateGridControl(_skillDayTemplate, _skill.TimeZone, _skill.DefaultResolution, _skill.SkillType);
			templateControl.Create();
			var gridToChartControl = new GridToChart(templateControl) {Dock = DockStyle.Fill};
			tableLayoutPanel1.Controls.Add(gridToChartControl,0,1);
			tableLayoutPanel1.SetColumnSpan(gridToChartControl,4);

			textBoxTemplateName.Text = _skillDayTemplate.Name;
		}

		public string TemplateName { get { return textBoxTemplateName.Text; } }

		private bool nameIsValid()
		{
			if (String.IsNullOrEmpty(textBoxTemplateName.Text.Trim()))
			{
				textBoxTemplateName.Text = _nameEmptyText;
				textBoxTemplateName.SelectAll();
			}
			if (textBoxTemplateName.Text == _nameEmptyText)
			{
				return false;
			}
			
			var skillDayTemplate = (ISkillDayTemplate) _skill.TryFindTemplateByName(TemplateTarget.Skill, textBoxTemplateName.Text);
			return (skillDayTemplate == null || 
					skillDayTemplate.Equals(_skillDayTemplate));
		}

		private void textBoxTemplateNameTextChanged(object sender, EventArgs e)
		{
			if (nameIsValid())
			{
				buttonAdvOK.Enabled = true;
				textBoxTemplateName.ForeColor = Color.FromKnownColor(KnownColor.WindowText);

				_toolTip.Hide(textBoxTemplateName);
			}
			else
			{
				buttonAdvOK.Enabled = false;
				if (textBoxTemplateName.Text != _nameEmptyText)
				{
					textBoxTemplateName.ForeColor = Color.Red;
					_toolTip.Show(UserTexts.Resources.NameAlreadyExists, textBoxTemplateName, new Point(textBoxTemplateName.Width - 30, -70), 5000);
				}
			}
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_skillDayTemplate.Name = textBoxTemplateName.Text;
			_skillDayTemplate.RefreshUpdatedDate();
			try
			{
				_skill.CheckRestrictions();
				PersistAll();
				var forecaster = Owner as Forecaster;
				if (forecaster != null)
					forecaster.RefreshTabs();
				Close();
			}
			catch (ValidationException validationException)
			{
				ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, validationException.Message), UserTexts.Resources.ValidationError);
			}
			DialogResult = DialogResult.OK;
		}

		private void releaseManagedResources()
		{
			_toolTip.Dispose();
		}
	}
}
