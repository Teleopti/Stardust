using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizerFindMatrixesForGroup
	{
		IList<IScheduleMatrixPro> Find(IPerson person, DateOnly dateOnly);
	}

	public class GroupOptimizerFindMatrixesForGroup : IGroupOptimizerFindMatrixesForGroup
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IList<IScheduleMatrixPro> _allMatrixes;

		public GroupOptimizerFindMatrixesForGroup(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, 
			IList<IScheduleMatrixPro> allMatrixes)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_allMatrixes = allMatrixes;
		}

		public IList<IScheduleMatrixPro> Find(IPerson person, DateOnly dateOnly)
		{
			IList<IScheduleMatrixPro> result = new List<IScheduleMatrixPro>();
			IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person, dateOnly);
			if (groupPerson == null)
			{
				//report that the person does not belong to any group and return false when broken out
				return result;
			}

			foreach (var member in groupPerson.GroupMembers)
			{
				foreach (var matrixPro in _allMatrixes)
				{
					if (matrixPro.Person.Equals(member))
					{
						if (matrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
						{
							result.Add(matrixPro);
						}
					}
				}
			}
			return result;
		}
	}
}