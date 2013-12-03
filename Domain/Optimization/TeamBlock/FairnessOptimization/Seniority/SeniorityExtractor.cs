using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ISeniorityExtractor
	{
		IList<ISeniorityInfo> ExtractSeniority(IList<ITeamInfo> teamInfos);
	}

	public class SeniorityExtractor : ISeniorityExtractor
	{
		public IList<ISeniorityInfo> ExtractSeniority(IList<ITeamInfo> teamInfos)
		{
			IList<ISeniorityInfo> seniorityInfos = new List<ISeniorityInfo>();
		
			var selectedPersons = new List<IPerson>();
			foreach (var teamInfo in teamInfos)
			{
				var groupPerson = teamInfo.GroupPerson;
				foreach (var groupMember in groupPerson.GroupMembers)
				{
					selectedPersons.Add(groupMember);
				}
			}

			//Extract seniority on person last name in PROTOTYPE
			var result = new Dictionary<IPerson, int>();
			var seniorityPoint = 0;
			foreach (var person in selectedPersons.OrderByDescending(s => s.Name.LastName))
			{
				result.Add(person, seniorityPoint);
				seniorityPoint++;
			}

			foreach (var teamInfo in teamInfos)
			{
				var totalSeniorityPoints = 0;
				var groupPerson = teamInfo.GroupPerson;
				foreach (var groupMember in groupPerson.GroupMembers)
				{
					var seniorityValue = result[groupMember];
					totalSeniorityPoints += seniorityValue;
				}

				var averageValue = totalSeniorityPoints / (double)groupPerson.GroupMembers.Count();
				var seniorityInfo = new SeniorityInfo(teamInfo, averageValue);
				seniorityInfos.Add(seniorityInfo);
			}
			//

			return seniorityInfos;
		}
	}
}
