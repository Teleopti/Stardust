using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupPersonBuilderForOptimization
	{
		Group BuildGroup(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnly dateOnly);
	}

	public class GroupPersonBuilderForOptimization : IGroupPersonBuilderForOptimization
	{
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly IGroupCreator _groupCreator;

		public GroupPersonBuilderForOptimization(Func<IGroupPagePerDateHolder> groupPagePerDateHolder, IGroupCreator groupCreator)
		{
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_groupCreator = groupCreator;
		}

		public Group BuildGroup(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnly dateOnly)
		{
			var groupPagePerDateHolder = _groupPagePerDateHolder();
			var pageOnDate = groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(dateOnly);
			var group = _groupCreator.CreateGroupForPerson(person, pageOnDate, personsInOrganisation.ToHashSet());

			return group;
		}
	}
}