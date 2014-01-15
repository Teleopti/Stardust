using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ISeniorityExtractor
	{
		IDictionary<ITeamInfo, ISeniorityInfo> ExtractSeniority(IList<ITeamInfo> teamInfos);
		IDictionary<ITeamBlockInfo, ISeniorityInfo> ExtractSeniority(IList<ITeamBlockInfo> teamInfos);
	}

	public class SeniorityExtractor : ISeniorityExtractor
	{
		public IDictionary<ITeamBlockInfo, ISeniorityInfo> ExtractSeniority(IList<ITeamBlockInfo> teamBlockInfos)
		{
			IDictionary<ITeamBlockInfo, ISeniorityInfo> seniorityInfos = new Dictionary<ITeamBlockInfo, ISeniorityInfo>();

			var selectedPersons = new List<IPerson>();

			foreach (var teamBlockInfo in teamBlockInfos)
			{
				var teamInfo = teamBlockInfo.TeamInfo;
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					selectedPersons.Add(groupMember);
				}
			}

			//Extract seniority on person last name in PROTOTYPE
			var result = new Dictionary<IPerson, int>();
			var seniorityPoint = 0;
			foreach (var person in selectedPersons.OrderByDescending(s => s.Name.LastName).Distinct())
			{
				result.Add(person, seniorityPoint);
				seniorityPoint++;
			}

			foreach (var teamBlockInfo in teamBlockInfos)
			{
				var teamInfo = teamBlockInfo.TeamInfo;
				var totalSeniorityPoints = 0;
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					var seniorityValue = result[groupMember];
					totalSeniorityPoints += seniorityValue;
				}

				var averageValue = totalSeniorityPoints / (double)teamInfo.GroupMembers.Count();
				var seniorityInfo = new SeniorityInfo(teamInfo, averageValue);
				//if (!seniorityInfos.ContainsKey(teamInfo))
				seniorityInfos.Add(teamBlockInfo, seniorityInfo);
			}
			//

			return seniorityInfos;
		}

		public IDictionary<ITeamInfo, ISeniorityInfo> ExtractSeniority(IList<ITeamInfo> teamInfos)
		{
			IDictionary<ITeamInfo, ISeniorityInfo> seniorityInfos = new Dictionary<ITeamInfo, ISeniorityInfo>();
		
			var selectedPersons = new List<IPerson>();
			foreach (var teamInfo in teamInfos)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					selectedPersons.Add(groupMember);
				}
			}

			//Extract seniority on person last name in PROTOTYPE
			var result = new Dictionary<IPerson, int>();
			var seniorityPoint = 0;
			foreach (var person in selectedPersons.OrderByDescending(s => s.Name.LastName).Distinct())
			{
				result.Add(person, seniorityPoint);
				seniorityPoint++;
			}

			foreach (var teamInfo in teamInfos)
			{
				var totalSeniorityPoints = 0;
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					var seniorityValue = result[groupMember];
					totalSeniorityPoints += seniorityValue;
				}

				var averageValue = totalSeniorityPoints / (double)teamInfo.GroupMembers.Count();
				var seniorityInfo = new SeniorityInfo(teamInfo, averageValue);
				if(!seniorityInfos.ContainsKey(teamInfo))
					seniorityInfos.Add(teamInfo, seniorityInfo);
			}
			//

			return seniorityInfos;
		}
	}
}
