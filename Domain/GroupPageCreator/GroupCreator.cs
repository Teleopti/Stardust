using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public interface IGroupCreator
	{
		Group CreateGroupForPerson(IPerson person, IGroupPage pageOnDate, IScheduleDictionary scheduleDictionary);
	}

	public class GroupCreator : IGroupCreator
	{
		public Group CreateGroupForPerson(IPerson person, IGroupPage pageOnDate, IScheduleDictionary scheduleDictionary)
		{
			var groupToReturn = new Group();

			var rootGroups = pageOnDate.RootGroupCollection;
			var rootPersonGroups = new List<IPersonGroup>();
			foreach (var rootPersonGroup in rootGroups)
			{
				rootPersonGroups.Add(rootPersonGroup);
			}

			IPersonGroup personGroup = currentGroupForPerson(rootPersonGroups, person);
			if (personGroup == null)
			{
				groupToReturn.AddMember(person);
				return groupToReturn;
			}

			var personsInGroup = currentPersonsInGroup(personGroup, scheduleDictionary);
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

		private IList<IPerson> currentPersonsInGroup(IPersonGroup personGroup, IScheduleDictionary scheduleDictionary)
		{
			var keys = scheduleDictionary.Keys;

			//in group and also in ScheduleDictionary
			var personsInDictionary = personGroup.PersonCollection.Where(keys.Contains).ToList();
			return personsInDictionary;
		}
	}
}