using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessGroupPersonHolder
	{
		IList<IGroupPerson> GroupPersons(IList<DateOnly> dateOnlyList, IGroupPageLight groupPage, DateOnly dateOnly, IEnumerable<IPerson> persons);
	}

	public class ShiftCategoryFairnessGroupPersonHolder : IShiftCategoryFairnessGroupPersonHolder
	{
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;

		private IGroupPageLight _groupPage;

		public ShiftCategoryFairnessGroupPersonHolder(IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider, IGroupPagePerDateHolder groupPagePerDateHolder,
			IGroupPersonsBuilder groupPersonsBuilder)
		{
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_groupPersonsBuilder = groupPersonsBuilder;
		}

		public IList<IGroupPerson> GroupPersons(IList<DateOnly> dateOnlyList, IGroupPageLight groupPage, DateOnly dateOnly, IEnumerable<IPerson> persons)
		{
			//only if we change grouping we need to redo this
			if (_groupPage == null || !_groupPage.Equals(groupPage))
			{
				_groupPage = groupPage;
				_groupPagePerDateHolder.GroupPersonGroupPagePerDate =
					_groupPageCreator.CreateGroupPagePerDate(dateOnlyList, _groupScheduleGroupPageDataProvider,
															 groupPage,true);
			}

			var groups = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, persons, false, null);
			return groups;
		}
	}
}