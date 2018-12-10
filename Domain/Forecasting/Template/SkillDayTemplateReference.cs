using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Template
{
    public class SkillDayTemplateReference : TemplateReference
    {
        private ISkill _skill;

        public SkillDayTemplateReference()
        {
        }

        public virtual ISkill Skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        public SkillDayTemplateReference(Guid templateId, int versionNumber, string name, DayOfWeek? weekday, ISkill skill)
            : base(templateId, versionNumber, name, weekday)
        {
            _skill = skill;
        }

        public override string TemplateName
        {
            get
            {
                return GetTemplateName();
            }
            set
            {
				base.TemplateName = value;
            }
        }

        private string GetTemplateName()
        {
            if (_skill == null)
				return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
									   UserTexts.Resources.None.ToUpper(CultureInfo.CurrentUICulture));		

            var templates =
                from t in _skill.TemplateWeekCollection
                where t.Value.Id == TemplateId
                select t;

            var templateList = templates.ToList();
            if (templateList.Count == 0)
            {
                if (base.TemplateName == LongtermTemplateKey)
					return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
										  UserTexts.Resources.Longterm.ToUpper(CultureInfo.CurrentUICulture));

				return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
									 UserTexts.Resources.Deleted.ToUpper(CultureInfo.CurrentUICulture));
            }

            IForecastDayTemplate template = templateList[0].Value;

            if (template.VersionNumber > VersionNumber)
            {
                var localUpdatedDateTime = TimeZoneHelper.ConvertFromUtc(UpdatedDate, _skill.TimeZone);
				return string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", TrimNameDecorations(template.Name), localUpdatedDateTime.ToShortDateString(), localUpdatedDateTime.ToShortTimeString());
            }
            return template.Name;
        }
    }
}
