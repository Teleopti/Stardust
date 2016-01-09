using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class VirtualSkillGroupsCreator
	{
		public VirtualSkillGroupsCreatorResult GroupOnDate(DateOnly dateOnly, IEnumerable<IPerson> personList)
		{
			var result = new VirtualSkillGroupsCreatorResult(dateOnly);
			foreach (var person in personList)
			{
				result.AddPerson(person);
			}

			return result;
		}
	}

	public class VirtualSkillGroupsCreatorResult
	{
		private readonly DateOnly _dateOnly;
		private readonly IDictionary<string, IList<IPerson>> _innerDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, IList<IPerson>> _innerSkillDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, string> _groupUniqueNameDic = new Dictionary<string, string>();

		public VirtualSkillGroupsCreatorResult(DateOnly dateOnly)
		{
			_dateOnly = dateOnly;
		}

		public void AddPerson(IPerson person)
		{
			var personPeriod = person.Period(_dateOnly);
			if (personPeriod == null)
				return;

			var key = string.Empty;
			foreach (var personSkill in personPeriod.PersonSkillCollection.OrderBy(s => s.Skill.Id))
			{
				if (personSkill.Active && !((IDeleteTag)personSkill.Skill).IsDeleted)
				{
					string thisId = personSkill.Skill.Id.ToString();
					key = key.IsEmpty() ? thisId : key + "|" + thisId;

					IList<IPerson> skillPersonList;
					if (_innerSkillDic.TryGetValue(thisId, out skillPersonList))
					{
						skillPersonList.Add(person);
					}
					else
					{
						_innerSkillDic.Add(thisId, new List<IPerson> { person });
					}
				}
			}

			if (key.IsEmpty())
				return;

			IList<IPerson> personList;
			if (_innerDic.TryGetValue(key, out personList))
			{
				personList.Add(person);
			}
			else
			{
				_innerDic.Add(key, new List<IPerson>{person});
				_groupUniqueNameDic.Add(key, "Group " + _innerDic.Count);
			}
		}

		public IEnumerable<string> GetKeys()
		{
			return _innerDic.Keys;
		}

		public string GetNameForKey(string key)
		{
			return _groupUniqueNameDic[key];
		}

		public IEnumerable<IPerson> GetPersonsForKey(string key)
		{
			return _innerDic[key];
		}

		public IEnumerable<IPerson> GetPersonsForSkillKey(string key)
		{
			IList<IPerson> personList;
			if (!_innerSkillDic.TryGetValue(key, out personList))
				return new List<IPerson>();

			return personList;
		}
	}
}