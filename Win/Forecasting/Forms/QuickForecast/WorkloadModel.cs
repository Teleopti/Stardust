using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class WorkloadModel
    {
        public IWorkload Workload { get; set; }

        public WorkloadModel(IWorkload workload)
        {
            Workload = workload;
        }

        public string Name { get { return Workload.Name; } }

        public string SkillName { get { return Workload.Skill.Name; } }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentUICulture,"{0} ({1})",Name,SkillName);
        }
    }
}