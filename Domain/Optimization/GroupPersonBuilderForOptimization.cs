using System;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupPersonBuilderForOptimization
	{
		Group BuildGroup(IPerson person, DateOnly dateOnly);
	}

	public class GroupPersonBuilderForOptimization : IGroupPersonBuilderForOptimization
	{
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly IGroupCreator _groupCreator;

		public GroupPersonBuilderForOptimization(Func<ISchedulingResultStateHolder> resultStateHolder, 
			Func<IGroupPagePerDateHolder> groupPagePerDateHolder, IGroupCreator groupCreator)
		{
			_resultStateHolder = resultStateHolder;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_groupCreator = groupCreator;
		}

		public Group BuildGroup(IPerson person, DateOnly dateOnly)
		{
			var groupPagePerDateHolder = _groupPagePerDateHolder();
			var pageOnDate = groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(dateOnly);
			var allPermittedPersons = _resultStateHolder().PersonsInOrganization;
			var group = _groupCreator.CreateGroupForPerson(person, pageOnDate, allPermittedPersons.ToList());

			return group;
		}
	}
}