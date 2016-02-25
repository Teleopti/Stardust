using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
    public interface IAnalyticsSkill
    {
        int SkillId { get; set; }
        Guid SkillCode { get; set; }
        string SkillName { get; set; }
        int TimeZoneId { get; set; }
        Guid ForecastMethodCode { get; set; }
        string ForecastMethodName { get; set; }
        int BusinessUnitId { get; set; }
        int DatasourceId { get; set; }
        DateTime InsertDate { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime DatasourceUpdateDate { get; set; }
        bool IsDeleted { get; set; }

    }
}
