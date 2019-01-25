﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public interface IGroupCreator
	{
		Group CreateGroupForPerson(IPerson person, IGroupPage pageOnDate, HashSet<IPerson> allPermittedPersons);
	}

	public class GroupCreator : IGroupCreator
	{
		public Group CreateGroupForPerson(IPerson person, IGroupPage pageOnDate, HashSet<IPerson> allPermittedPersons)
		{
			var groupToReturn = new Group();

			var rootPersonGroups = pageOnDate.RootGroupCollection.ToList();

			IPersonGroup personGroup = currentGroupForPerson(rootPersonGroups, person);
			if (personGroup != null)
			{
				groupToReturn.SetName(personGroup.Name);
			}
			else
			{
				if (!allPermittedPersons.Contains(person))
					return null;

				groupToReturn.AddMember(person);
				groupToReturn.SetName(person.Name.ToString());
				return groupToReturn;
			}

			var personsInGroup = currentPersonsInGroup(personGroup, allPermittedPersons);
			groupToReturn.AddMembers(personsInGroup);
			
			return groupToReturn;
		}

		private IPersonGroup currentGroupForPerson(IEnumerable<IPersonGroup> personGroups, IPerson person)
		{
			foreach (var personGroup in personGroups)
			{
				if (personGroup.PersonCollection.Contains(person))
				{
					return personGroup;
				}

				var childGroups = personGroup.ChildGroupCollection.ToList();
				IPersonGroup result = currentGroupForPerson(childGroups, person);
				if (result != null)
					return result;
			}

			return null;
		}

		private IList<IPerson> currentPersonsInGroup(IPersonGroup personGroup, HashSet<IPerson> allPermittedPersons)
		{
			var personsToReturn = personGroup.PersonCollection.Where(allPermittedPersons.Contains).ToList();
			return personsToReturn;
		}
	}
}