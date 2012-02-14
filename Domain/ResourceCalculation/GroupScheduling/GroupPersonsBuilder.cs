using System;
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
		private readonly IGroupPersonShiftCategoryConsistentChecker _groupPersonShiftCategoryConsistentChecker;
		private readonly IWorkShiftFinderResultHolder _finderResultHolder;
	    private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;

	    public GroupPersonsBuilder(ISchedulingResultStateHolder resultStateHolder, IGroupPersonFactory groupPersonFactory, IGroupPersonShiftCategoryConsistentChecker groupPersonShiftCategoryConsistentChecker, 
				 IWorkShiftFinderResultHolder finderResultHolder, IGroupPagePerDateHolder groupPagePerDateHolder)
		{
			_resultStateHolder = resultStateHolder;
            _groupPersonFactory = groupPersonFactory;
			_groupPersonShiftCategoryConsistentChecker = groupPersonShiftCategoryConsistentChecker;
			_finderResultHolder = finderResultHolder;
		    _groupPagePerDateHolder = groupPagePerDateHolder;
		}

        public IList<IGroupPerson> BuildListOfGroupPersons(DateOnly dateOnly, IList<IPerson> selectedPersons, bool checkShiftCategoryConsistency)
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

			checkGroupCollection(dateOnly, personGroups, retLis, checkShiftCategoryConsistency);

			return retLis;
		}

        private void checkGroupCollection(DateOnly dateOnly, IEnumerable<IPersonGroup> groups, List<IGroupPerson> retLis, bool checkShiftCategoryConsistency)
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
                    if (checkShiftCategoryConsistency && !_groupPersonShiftCategoryConsistentChecker.AllPersonsHasSameOrNoneShiftCategoryScheduled(dic, personsInDictionary, dateOnly))
                    {
                        addResult(personGroup.Description.Name, dateOnly);

                        continue;
                    }
               
                    var newGroupPerson = _groupPersonFactory.CreateGroupPerson(personsInGroup, dateOnly, personGroup.Description.Name);
                    if (!newGroupPerson.GroupMembers.IsEmpty())
                    {
                        newGroupPerson.CommonShiftCategory = _groupPersonShiftCategoryConsistentChecker.CommonShiftCategory;
                        retLis.Add(newGroupPerson);
                    }
                }
				
				var personGroups = new List<IPersonGroup>();
				foreach (var rootPersonGroup in personGroup.ChildGroupCollection)
				{
					personGroups.Add(rootPersonGroup);
				}
				checkGroupCollection(dateOnly, personGroups, retLis, checkShiftCategoryConsistency);
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
