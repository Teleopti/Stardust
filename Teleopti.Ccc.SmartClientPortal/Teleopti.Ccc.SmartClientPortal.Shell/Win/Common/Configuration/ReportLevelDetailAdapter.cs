using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
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
