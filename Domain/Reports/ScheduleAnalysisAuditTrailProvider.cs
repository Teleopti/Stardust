using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Reports
{
	public class ScheduleAnalysisAuditTrailProvider : IScheduleAnalysisProvider
	{

		public List<IApplicationFunction> GetScheduleAnalysisApplicationFunctions(IList<IApplicationFunction> applicationFunctions)
		{
			var ret = (from a in applicationFunctions
				where analysisReports().Contains(a.ForeignId.ToUpper())
				select a).ToList();

			var auditApplicationFunction = PrincipalAuthorization.Current_DONTUSE().GrantedFunctions()
				.FirstOrDefault(a => a.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport);
			if (auditApplicationFunction != null)
			{
				auditApplicationFunction.IsWebReport = true;
				ret.Add(auditApplicationFunction);
			}

			return ret;
		}

		private static IEnumerable<string> analysisReports()
		{
			// old "21", "18", "17", "19", "26", "29"
			return new[]
			{
				"132E3AF2-3557-4EA7-813E-05CD4869D5DB",
				"63243F7F-016E-41D1-9432-0787D26F9ED5",
				"009BCDD2-3561-4B59-A719-142CD9216727",
				"BAA446C2-C060-4F39-83EA-B836B1669331",
				"2F222F0A-4571-4462-8FBE-0C747035994A",
				"C052796F-1C8A-4905-9246-FF1FF8BD30E5"
			};
		}

	}
}