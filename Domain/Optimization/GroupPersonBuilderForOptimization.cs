﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupPersonBuilderForOptimization
	{
		IGroupPerson BuildGroupPerson(IPerson person, DateOnly dateOnly);
	}

	public class GroupPersonBuilderForOptimization : IGroupPersonBuilderForOptimization
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IGroupPersonFactory _groupPersonFactory;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;

		public GroupPersonBuilderForOptimization(ISchedulingResultStateHolder resultStateHolder, IGroupPersonFactory groupPersonFactory, 
			IGroupPagePerDateHolder groupPagePerDateHolder)
		{
			_resultStateHolder = resultStateHolder;
			_groupPersonFactory = groupPersonFactory;
			_groupPagePerDateHolder = groupPagePerDateHolder;
		}

		public IGroupPerson BuildGroupPerson(IPerson person, DateOnly dateOnly)
		{
			var pageOnDate = _groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(dateOnly);
			var rootGroups = pageOnDate.RootGroupCollection;
			var rootPersonGroups = new List<IPersonGroup>();
			foreach (var rootPersonGroup in rootGroups)
			{
				rootPersonGroups.Add(rootPersonGroup);
			}

			IPersonGroup personGroup = currentGroupForPerson(rootPersonGroups, person);

			if (personGroup == null)
			{
				return null;
			}

			var personsInGroup = curentPersonsInGroup(personGroup);
			var guid = ((IEntity) personGroup).Id;
			var groupPerson = _groupPersonFactory.CreateGroupPerson(personsInGroup, dateOnly, personGroup.Description.Name, guid);
			
			return groupPerson;
		}

		private IList<IPerson> curentPersonsInGroup(IPersonGroup personGroup)
		{
			var dic = _resultStateHolder.Schedules;
			var keys = dic.Keys;

			//in group and also in ScheduleDictionary
			var personsInDictionary = personGroup.PersonCollection.Where(keys.Contains).ToList();
			return personsInDictionary;
		}

		private IPersonGroup currentGroupForPerson(IEnumerable<IPersonGroup> personGroups, IPerson person)
		{
			foreach (var personGroup in personGroups)
			{
				if (personGroup.PersonCollection.Contains(person))
				{
					return personGroup;
				}

				var childGroups = new List<IPersonGroup>();
				foreach (var rootPersonGroup in personGroup.ChildGroupCollection)
				{
					childGroups.Add(rootPersonGroup);
				}

				IPersonGroup result = currentGroupForPerson(childGroups, person);
				if(result != null)
					return result;

			}

			return null;
		}
	}
}