using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Template
{
    public class MultisiteDayTemplateReference : TemplateReference
    {
        private IMultisiteSkill _multisiteSkill;

        public MultisiteDayTemplateReference()
        {
        }

        public virtual IMultisiteSkill MultisiteSkill
        {
            get { return _multisiteSkill; }
            set { _multisiteSkill = value; }
        }

        public MultisiteDayTemplateReference(Guid templateId, int versionNumber, string name, DayOfWeek? weekday, IMultisiteSkill skill)
            : base(templateId, versionNumber, name, weekday)
        {
            _multisiteSkill = skill;
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
			if (_multisiteSkill == null)
				return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
				                     UserTexts.Resources.None.ToUpper(CultureInfo.CurrentUICulture));

        	var templates =
                from t in _multisiteSkill.TemplateMultisiteWeekCollection
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
				if(UpdatedDate == SkillDayTemplate.BaseDate.Date)
					return string.Format(CultureInfo.CurrentUICulture, "<{0} BASE>", TrimNameDecorations(template.Name));
                var localUpdatedDateTime = TimeZoneHelper.ConvertFromUtc(UpdatedDate, _multisiteSkill.TimeZone);
				return string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", TrimNameDecorations(template.Name), localUpdatedDateTime.ToShortDateString(), localUpdatedDateTime.ToShortTimeString());
            }
            return template.Name;
        }
    }
}
