using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ISeniorityExtractor
	{
        IList<ITeamBlockPoints> ExtractSeniority(IList<ITeamBlockInfo> teamInfos);
	}

	public class SeniorityExtractor : ISeniorityExtractor
	{
	    private readonly IRankedPersonBasedOnStartDate  _rankedPersonBasedOnStartDate;

	    public SeniorityExtractor(IRankedPersonBasedOnStartDate rankedPersonBasedOnStartDate)
	    {
	        _rankedPersonBasedOnStartDate = rankedPersonBasedOnStartDate;
	    }

	    public IList<ITeamBlockPoints> ExtractSeniority(IList<ITeamBlockInfo> teamBlockInfos)
        {
            var seniorityInfos = new List<ITeamBlockPoints>();

            var selectedPersons = new List<IPerson>();

            foreach (var teamBlockInfo in teamBlockInfos)
            {
                var teamInfo = teamBlockInfo.TeamInfo;
                foreach (var groupMember in teamInfo.GroupMembers)
                {
                    if(!selectedPersons.Contains(groupMember ))
                        selectedPersons.Add(groupMember);
                }
            }

            //Extract seniority on person last name in PROTOTYPE
            var result = new Dictionary<IPerson, int>();
            var seniorityPoint = 0;
            foreach (var person in _rankedPersonBasedOnStartDate.GetRankedPerson(selectedPersons ))
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
                var seniorityInfo = new TeamBlockPoints(teamBlockInfo, averageValue);
                //if (!seniorityInfos.ContainsKey(teamInfo))
                seniorityInfos.Add(seniorityInfo);
            }
            //

            return seniorityInfos;
        }

	}
}
