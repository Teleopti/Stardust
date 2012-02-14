using System.Collections.Generic;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public interface ISkillProvider
	{
		IEnumerable<ReportControlSkillGet> GetAvailableSkills();
	}
}