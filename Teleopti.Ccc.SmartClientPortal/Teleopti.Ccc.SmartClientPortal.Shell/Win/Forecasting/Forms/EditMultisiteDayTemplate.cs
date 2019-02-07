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
	public partial class EditMultisiteDayTemplate : BaseRibbonFormWithUnitOfWork
	{
		private readonly IMultisiteSkill _multisiteSkill;
		private IMultisiteDayTemplate _multisiteDayTemplate;
		private readonly string _nameEmptyText = UserTexts.Resources.EnterNameHere;
		private readonly ToolTip _toolTip = new ToolTip();

		public EditMultisiteDayTemplate()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			_toolTip.IsBalloon = true;
			_toolTip.InitialDelay = 1000;
			_toolTip.ToolTipTitle = UserTexts.Resources.InvalidAgentName;
		}

		public EditMultisiteDayTemplate(IMultisiteDayTemplate multisiteDayTemplate) : this()
		{
			var skillRepository = new MultisiteSkillRepository(UnitOfWork);
			_multisiteSkill = skillRepository.Get(multisiteDayTemplate.Parent.Id.GetValueOrDefault());
			_multisiteDayTemplate = (IMultisiteDayTemplate) _multisiteSkill.TryFindTemplateByName(TemplateTarget.Multisite, multisiteDayTemplate.Name);
		}

		public EditMultisiteDayTemplate(IMultisiteSkill skill) : this()
		{
			var skillRepository = new MultisiteSkillRepository(UnitOfWork);
			_multisiteSkill = skillRepository.Get(skill.Id.GetValueOrDefault());

			var distribution = initializeDistribution();
			var timePeriod = initializeTimePeriod();

			createNewMultisiteDayTemplate(timePeriod, distribution);
		}
		
		public EditMultisiteDayTemplate(IMultisiteDay multisiteDay) : this()
		{
			if (multisiteDay == null) throw new ArgumentNullException("multisiteDay");
			var skillRepository = new MultisiteSkillRepository(UnitOfWork);
			_multisiteSkill = skillRepository.Get(multisiteDay.Skill.Id.GetValueOrDefault());

			var distribution = initializeDistribution();
			var timePeriod = initializeTimePeriod();

			createNewMultisiteDayTemplate(timePeriod, distribution);
			initializeMultisiteDayTemplateFromMultisiteDay(multisiteDay);
		}

		private void initializeMultisiteDayTemplateFromMultisiteDay(IMultisiteDay multisiteDay)
		{
			var baseDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _multisiteSkill.TimeZone);
			var templateMultisitePeriod = multisiteDay.MultisitePeriodCollection.Select(
				multisitePeriod => new TemplateMultisitePeriod(getBaseDateTimePeriod(multisitePeriod.Period, ref baseDateUtc), multisitePeriod.Distribution)).Cast<ITemplateMultisitePeriod>().ToList();
			templateMultisitePeriod.ForEach(x => x.IsDistributionChangeNotAllowed = true);
			_multisiteDayTemplate.SetMultisitePeriodCollection(templateMultisitePeriod);
		}

		private void createNewMultisiteDayTemplate(DateTimePeriod timePeriod, IDictionary<IChildSkill, Percent> distribution)
		{
			ITemplateMultisitePeriod templateMultisitePeriod = new TemplateMultisitePeriod(timePeriod, distribution);
			_multisiteDayTemplate = new MultisiteDayTemplate("New Template",
															 new List<ITemplateMultisitePeriod>
																{
																	templateMultisitePeriod
																});
			textBoxTemplateName.ReadOnly = false;
			_multisiteSkill.AddTemplate(_multisiteDayTemplate);
		}

		private static DateTimePeriod getBaseDateTimePeriod(DateTimePeriod period, ref DateTime baseDateUtc)
		{
			var startTimeUtc = period.StartDateTime;
			var endTimeUtc = period.EndDateTime;

			var startDateTimeUtc = new DateTime(baseDateUtc.Year, baseDateUtc.Month, baseDateUtc.Day, startTimeUtc.Hour, startTimeUtc.Minute, startTimeUtc.Second, DateTimeKind.Utc);
			if (endTimeUtc.Date > startTimeUtc.Date)
				baseDateUtc = baseDateUtc.AddDays(1);
			var endDateTimeUtc = new DateTime(baseDateUtc.Year, baseDateUtc.Month, baseDateUtc.Day, endTimeUtc.Hour, endTimeUtc.Minute, endTimeUtc.Second, DateTimeKind.Utc);

			return new DateTimePeriod(startDateTimeUtc, endDateTimeUtc);
		}

		private DateTimePeriod initializeTimePeriod()
		{
			var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _multisiteSkill.TimeZone);
			return new DateTimePeriod(startDateUtc,startDateUtc.AddDays(1)).MovePeriod(_multisiteSkill.MidnightBreakOffset);
		}

		private Dictionary<IChildSkill, Percent> initializeDistribution()
		{
			var distribution = new Dictionary<IChildSkill, Percent>();
			var childCount = _multisiteSkill.ChildSkills.Count;
			if (childCount == 0) return distribution;

			int remainder;
			var value = Math.DivRem(100, childCount, out remainder);

			foreach (var child in _multisiteSkill.ChildSkills)
			{
				distribution.Add(child, new Percent((value + remainder) / 100d));
				remainder = 0;
			}
			return distribution;
		}

		private void editTemplateLoad(object sender, EventArgs e)
		{
			var templateControl = new MultisiteIntradayTemplateGridControl(_multisiteSkill, _multisiteDayTemplate, _multisiteSkill.TimeZone, _multisiteSkill.DefaultResolution);
			templateControl.Create();
			var gridToChartControl = new GridToChart(templateControl);
			gridToChartControl.Dock = DockStyle.Fill;
			tableLayoutPanel1.Controls.Add(gridToChartControl,0,1);
			tableLayoutPanel1.SetColumnSpan(gridToChartControl,4);

			textBoxTemplateName.Text = _multisiteDayTemplate.Name;
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
			
			var multisiteDayTemplate = _multisiteSkill.TryFindTemplateByName(TemplateTarget.Multisite, textBoxTemplateName.Text) as IMultisiteDayTemplate;
			return (multisiteDayTemplate == null || multisiteDayTemplate.Equals(_multisiteDayTemplate));
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
			_multisiteDayTemplate.Name = textBoxTemplateName.Text;
			_multisiteDayTemplate.RefreshUpdatedDate();
			try
			{
				_multisiteSkill.CheckRestrictions();
				PersistAll();
				var forecaster = Owner as Forecaster;
				if (forecaster != null)
					forecaster.RefreshTabs();
				Close();
				DialogResult = DialogResult.OK;
			}
			catch (ValidationException validationException)
			{
				ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, validationException.Message), UserTexts.Resources.ValidationError);
			}
		}

		private void ReleaseManagedResources()
		{
			_toolTip.Dispose();
		}
	}
}
