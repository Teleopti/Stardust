using System.Collections.Generic;
using System.Linq;
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
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

		public GroupOptimizerValidateProposedDatesInSameGroup(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, 
			IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ValidatorResult Validate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff)
		{
			ValidatorResult result = new ValidatorResult();

			if (dates.Count < 2)
			{
				result.Success = true;
				return result;
			}

			if(useSameDaysOff)
			{
				DateOnly firstDate = dates[0];
				IGroupPerson firstGroup = _groupPersonBuilderForOptimization.BuildGroupPerson(person, firstDate);
				var firstGroupMembers = firstGroup.GroupMembers;
				
				for (int i = 1; i < dates.Count; i++)
				{
					IGroupPerson thisGroup = _groupPersonBuilderForOptimization.BuildGroupPerson(person, dates[i]);
					var groupMembersToCompare = thisGroup.GroupMembers;
					if (firstGroupMembers.Count() != groupMembersToCompare.Count())
					{
						result.Success = true;
						result.DaysToLock = new DateOnlyPeriod(dates[0], dates[0]);
						result.MatrixList = _groupOptimizerFindMatrixesForGroup.Find(person, dates[0]);
						return result;
					}

					foreach (var member in firstGroupMembers)
					{
						if (!groupMembersToCompare.Contains(member))
						{
							result.Success = true;
							result.DaysToLock = new DateOnlyPeriod(dates[0], dates[0]);
							result.MatrixList = _groupOptimizerFindMatrixesForGroup.Find(person, dates[0]);
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