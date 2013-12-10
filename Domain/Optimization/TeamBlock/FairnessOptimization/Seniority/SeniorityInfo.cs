using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface  ISeniorityInfo
	{
		ITeamInfo TeamInfo { get; }
		double Seniority { get; }
	}
	public class SeniorityInfo : ISeniorityInfo
	{
		public SeniorityInfo(ITeamInfo teamInfo, double seniority)
		{
			TeamInfo = teamInfo;
			Seniority = seniority;
		}

		public ITeamInfo TeamInfo { get; private set; }
		public double Seniority { get; private set; }
	}
}
