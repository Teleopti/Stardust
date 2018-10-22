using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public class IntraIntervalIssues
	{
		public IntraIntervalIssues() 
		{
			IssuesOnDay = new List<ISkillStaffPeriod>();
			IssuesOnDayAfter = new List<ISkillStaffPeriod>();
		}

		public IList<ISkillStaffPeriod> IssuesOnDay { get; set; }
		public IList<ISkillStaffPeriod> IssuesOnDayAfter { get; set; }
	}
}