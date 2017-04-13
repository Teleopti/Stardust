using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class ReportLevelDetailAdapter
    {
         public string DisplayName { get; set; }

         public ReportLevelDetail ReportLevelDetail { get; set; }

         public ReportLevelDetailAdapter(string displayName, ReportLevelDetail reportLevelDetail)
        {
             DisplayName = displayName;
             ReportLevelDetail = reportLevelDetail;
        }
    }
}
