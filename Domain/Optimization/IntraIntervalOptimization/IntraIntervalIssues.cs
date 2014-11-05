using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IIntraIntervalIssues
	{
		IList<ISkillStaffPeriod> IssuesOnDay { get; set; }
		IList<ISkillStaffPeriod> IssuesOnDayAfter { get; set; }
	}

	public class IntraIntervalIssues : IIntraIntervalIssues
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