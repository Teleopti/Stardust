using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class GroupPersonsBuilder : IGroupPersonsBuilder
	{
		private IEnumerable<IPerson> _selectedPersons;
		private readonly ISchedulingResultStateHolder _resultStateHolder;
        
		private readonly IGroupPersonFactory _groupPersonFactory;
		private readonly IWorkShiftFinderResultHolder _finderResultHolder;
	    private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;

		public GroupPersonsBuilder(ISchedulingResultStateHolder resultStateHolder, IGroupPersonFactory groupPersonFactory,
				 IWorkShiftFinderResultHolder finderResultHolder, IGroupPagePerDateHolder groupPagePerDateHolder)
		{
			_resultStateHolder = resultStateHolder;
            _groupPersonFactory = groupPersonFactory;
			_finderResultHolder = finderResultHolder;
		    _groupPagePerDateHolder = groupPagePerDateHolder;
		}

		public IList<IGroupPerson> BuildListOfGroupPersons(DateOnly dateOnly, IEnumerable<IPerson> selectedPersons, bool checkShiftCategoryConsistency, ISchedulingOptions schedulingOptions)
		{
			_selectedPersons = selectedPersons;
			var pageOnDate = _groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(dateOnly);
			var rootGroups = new List<IRootPersonGroup>();
			rootGroups.AddRange(pageOnDate.RootGroupCollection);
			var personsInSelectedGroupPage = new List<IPerson>();
			var dic = _resultStateHolder.Schedules;
			var keys = dic.Keys;
			foreach (var personGroup in pageOnDate.RootGroupCollection)
			{
				var personsInDictionary = personGroup.PersonCollection.Where(keys.Contains).ToList();
				personsInSelectedGroupPage.AddRange(personsInDictionary);
			}
			var ungroupedPersons = _selectedPersons.Where(x => !personsInSelectedGroupPage.Contains(x)).ToList();
			if (ungroupedPersons.Count > 0)
			{
				var converter = new PersonToSingleAgentTeamRootPersonGroupConverter(new SingleAgentTeamGroupPage());
				foreach (var ungroupedPerson in ungroupedPersons)
				{
					var personGroup = converter.Convert(ungroupedPerson, dateOnly);
					rootGroups.Add(personGroup);
				}
			}

			var retLis = new List<IGroupPerson>();
			var personGroups = new List<IPersonGroup>();
			foreach (var rootPersonGroup in rootGroups)
			{
				personGroups.Add(rootPersonGroup);
			}

			checkGroupCollection(dateOnly, personGroups, retLis, checkShiftCategoryConsistency, schedulingOptions);

			return retLis;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void checkGroupCollection(DateOnly dateOnly, IEnumerable<IPersonGroup> groups, List<IGroupPerson> retLis, bool checkShiftCategoryConsistency, ISchedulingOptions schedulingOptions)
		{
            if (groups.IsEmpty())
		    return;
		    var dic = _resultStateHolder.Schedules;
            var keys = dic.Keys;
			foreach (var personGroup in groups)
			{
                //in group and also in ScheduleDictionary
                var personsInDictionary = personGroup.PersonCollection.Where(keys.Contains).ToList();

                //In group, in Dictionary and Selected
                var personsInGroup = personsInDictionary.Where(person => _selectedPersons.Contains(person)).ToList();
                if (!personsInGroup.IsEmpty())
                {
                    if (checkShiftCategoryConsistency)
                    {
                        addResult(personGroup.Description.Name, dateOnly);

                        continue;
                    }

					var guid = ((IEntity)personGroup).Id;

					if (guid == null && personGroup.GetType() == typeof(ChildPersonGroup))
					{
						var entity = ((ChildPersonGroup)personGroup).Entity;
						if (entity != null && entity.Id.HasValue)
							guid = entity.Id.Value;
					}

					if (guid == null && personGroup.GetType() == typeof(RootPersonGroup))
					{
						var entity = ((RootPersonGroup)personGroup).Entity;
						if (entity != null && entity.Id.HasValue)
							guid = entity.Id.Value;
					}
					if (guid == null) guid = Guid.Empty;

                    var newGroupPerson = _groupPersonFactory.CreateGroupPerson(personsInGroup, dateOnly, personGroup.Description.Name, guid);
                    if (!newGroupPerson.GroupMembers.IsEmpty())
                    {
                        retLis.Add(newGroupPerson);
                    }
                }
				
				var personGroups = new List<IPersonGroup>();
				foreach (var rootPersonGroup in personGroup.ChildGroupCollection)
				{
					personGroups.Add(rootPersonGroup);
				}
				checkGroupCollection(dateOnly, personGroups, retLis, checkShiftCategoryConsistency, schedulingOptions);
			}
		}

		private void addResult(string name, DateOnly dateOnly)
		{
			IPerson dummyPerson = new Person{Name = new Name(name,"")};
			dummyPerson.SetId(Guid.NewGuid());
			_finderResultHolder.AddFilterToResult(dummyPerson, dateOnly, UserTexts.Resources.ErrorMessageNotConsistentShiftCategories);
		}
	}
}
