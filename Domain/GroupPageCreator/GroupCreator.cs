using System.Collections.Generic;
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

			var rootGroups = pageOnDate.RootGroupCollection;
			var rootPersonGroups = new List<IPersonGroup>();
			foreach (var rootPersonGroup in rootGroups)
			{
				rootPersonGroups.Add(rootPersonGroup);
			}

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
			foreach (var personInGroup in personsInGroup)
			{
				groupToReturn.AddMember(personInGroup);
			}

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

				var childGroups = new List<IPersonGroup>();
				foreach (var rootPersonGroup in personGroup.ChildGroupCollection)
				{
					childGroups.Add(rootPersonGroup);
				}

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