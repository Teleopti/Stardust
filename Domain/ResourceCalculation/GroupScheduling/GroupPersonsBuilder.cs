﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class GroupPersonsBuilder : IGroupPersonsBuilder
	{
		private IList<IPerson> _selectedPersons;
		private readonly ISchedulingResultStateHolder _resultStateHolder;
        
		private readonly IGroupPersonFactory _groupPersonFactory;
		private readonly IGroupPersonConsistentChecker _groupPersonConsistentChecker;
		private readonly IWorkShiftFinderResultHolder _finderResultHolder;
	    private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;

		public GroupPersonsBuilder(ISchedulingResultStateHolder resultStateHolder, IGroupPersonFactory groupPersonFactory, IGroupPersonConsistentChecker groupPersonConsistentChecker, 
				 IWorkShiftFinderResultHolder finderResultHolder, IGroupPagePerDateHolder groupPagePerDateHolder)
		{
			_resultStateHolder = resultStateHolder;
            _groupPersonFactory = groupPersonFactory;
			_groupPersonConsistentChecker = groupPersonConsistentChecker;
			_finderResultHolder = finderResultHolder;
		    _groupPagePerDateHolder = groupPagePerDateHolder;
		}

		public IList<IGroupPerson> BuildListOfGroupPersons(DateOnly dateOnly, IList<IPerson> selectedPersons, bool checkShiftCategoryConsistency, ISchedulingOptions schedulingOptions)
		{
            _selectedPersons = selectedPersons;
			var pageOnDate = _groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(dateOnly);
			var rootGroups = pageOnDate.RootGroupCollection;
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
                    if (checkShiftCategoryConsistency && !_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(dic, personsInDictionary, dateOnly, schedulingOptions))
                    {
                        addResult(personGroup.Description.Name, dateOnly);

                        continue;
                    }

					var guid = ((IEntity)personGroup).Id;

					if (personGroup.GetType() == typeof(ChildPersonGroup))
					{
						var entity = ((ChildPersonGroup)personGroup).Entity;
						if (entity.Id.HasValue)
							guid = entity.Id.Value;
					}

					if (personGroup.GetType() == typeof(RootPersonGroup))
					{
						var entity = ((RootPersonGroup)personGroup).Entity;
						if (entity.Id.HasValue)
							guid = entity.Id.Value;
					}

                    var newGroupPerson = _groupPersonFactory.CreateGroupPerson(personsInGroup, dateOnly, personGroup.Description.Name, guid);
                    if (!newGroupPerson.GroupMembers.IsEmpty())
                    {
                        newGroupPerson.CommonPossibleStartEndCategory = _groupPersonConsistentChecker.CommonPossibleStartEndCategory;
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
