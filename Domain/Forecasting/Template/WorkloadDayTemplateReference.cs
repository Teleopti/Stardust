using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Template
{
    public class WorkloadDayTemplateReference : TemplateReference
    {
        private IWorkload _workload;

        public WorkloadDayTemplateReference()
        {
        }

        public virtual IWorkload Workload
        {
            get{ return _workload; }
            set{ _workload = value; }
        }

        public WorkloadDayTemplateReference(Guid templateId, int versionNumber, string name, DayOfWeek? weekday, IWorkload workload)
            : base(templateId, versionNumber, name, weekday)
        {
            _workload = workload;
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
			if (_templateName == WebTemplateKey)
				return _templateName;

			if (_workload == null)
                return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
                                     UserTexts.Resources.None.ToUpper(CultureInfo.CurrentUICulture));

            var templates = _workload.TemplateWeekCollection.Where(t => t.Value.Id == TemplateId);
            if (templates.IsEmpty())
            {
                if (base.TemplateName == LongtermTemplateKey)
                    return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
                                         UserTexts.Resources.Longterm.ToUpper(CultureInfo.CurrentUICulture));

                return string.Format(CultureInfo.CurrentUICulture, TemplateNameFormat,
                                     UserTexts.Resources.Deleted.ToUpper(CultureInfo.CurrentUICulture));
            }

            var template = templates.First().Value;
            if (template.VersionNumber > VersionNumber)
            {
				if (UpdatedDate == new DateTime())
					UpdatedDate = template.CreatedDate;
                var localUpdatedDateTime = TimeZoneHelper.ConvertFromUtc(UpdatedDate, _workload.Skill.TimeZone);
				return string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", TrimNameDecorations(template.Name), localUpdatedDateTime.ToShortDateString(), localUpdatedDateTime.ToShortTimeString());
            }
            return template.Name;
        }
    }
}
