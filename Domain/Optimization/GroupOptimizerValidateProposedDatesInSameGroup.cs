using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizerValidateProposedDatesInSameGroup
	{
		ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);
	}

	public class GroupOptimizerValidateProposedDatesInSameGroup : IGroupOptimizerValidateProposedDatesInSameGroup
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		public GroupOptimizerValidateProposedDatesInSameGroup(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		}

		public ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff)
		{
			ValidatorResult result = new ValidatorResult();

			if(useSameDaysOff)
			{
				DateOnly firstDate = dates[0];
				IGroupPerson firstGroup = _groupPersonBuilderForOptimization.BuildGroupPerson(person, firstDate);
				for (int i = 1; i < dates.Count; i++)
				{
					IGroupPerson groupToCompare = _groupPersonBuilderForOptimization.BuildGroupPerson(person, dates[i]);
					if(firstGroup.GroupMembers.Count != groupToCompare.GroupMembers.Count)
					{
						result.Success = true;
						result.DaysToLock = new DateOnlyPeriod(dates[0], dates[0]);
						return result;
					}

					foreach (var member in firstGroup.GroupMembers)
					{
						if(!groupToCompare.GroupMembers.Contains(member))
						{
							result.Success = true;
							result.DaysToLock = new DateOnlyPeriod(dates[0], dates[0]);
							return result;
						}
					}
				}
			}

			result.Success = true;
			return result;
		}
	}
}