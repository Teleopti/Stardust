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
			var skillGroupKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
			var skillKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
			var skillGroupKeyUniqueNameDic = new Dictionary<string, string>();
			foreach (var person in personList)
			{
				addPerson(person, dateOnly, skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic);
			}

			return new VirtualSkillGroupsCreatorResult(skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic);
		}

		private static void addPerson(IPerson person, DateOnly date, IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic)
		{
			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return;

			var key = string.Empty;
			foreach (var personSkill in personPeriod.PersonSkillCollection.OrderBy(s => s.Skill.Id))
			{
				if (personSkill.Active && !((IDeleteTag) personSkill.Skill).IsDeleted)
				{
					string thisId = personSkill.Skill.Id.ToString();
					key = key.IsEmpty() ? thisId : key + "|" + thisId;

					IList<IPerson> skillPersonList;
					if (skillKeyPersonListDic.TryGetValue(thisId, out skillPersonList))
					{
						skillPersonList.Add(person);
					}
					else
					{
						skillKeyPersonListDic.Add(thisId, new List<IPerson> {person});
					}
				}
			}

			if (key.IsEmpty())
				return;

			IList<IPerson> personList;
			if (skillGroupKeyPersonListDic.TryGetValue(key, out personList))
			{
				personList.Add(person);
			}
			else
			{
				skillGroupKeyPersonListDic.Add(key, new List<IPerson> {person});
				skillGroupKeyUniqueNameDic.Add(key, "Group " + skillGroupKeyPersonListDic.Count);
			}
		}
	}

	public class VirtualSkillGroupsCreatorResult
	{
		private readonly IDictionary<string, IList<IPerson>> _skillGroupKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, IList<IPerson>> _skillKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, string> _skillGroupKeyUniqueNameDic = new Dictionary<string, string>();

		public VirtualSkillGroupsCreatorResult(IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic)
		{
			_skillGroupKeyPersonListDic = skillGroupKeyPersonListDic;
			_skillKeyPersonListDic = skillKeyPersonListDic;
			_skillGroupKeyUniqueNameDic = skillGroupKeyUniqueNameDic;
		}

		public IEnumerable<string> GetKeys()
		{
			return _skillGroupKeyPersonListDic.Keys;
		}

		public string GetNameForKey(string key)
		{
			return _skillGroupKeyUniqueNameDic[key];
		}

		public IEnumerable<IPerson> GetPersonsForSkillGroupKey(string key)
		{
			return _skillGroupKeyPersonListDic[key];
		}

		public IEnumerable<IPerson> GetPersonsForSkillKey(string key)
		{
			IList<IPerson> personList;
			if (!_skillKeyPersonListDic.TryGetValue(key, out personList))
				return new List<IPerson>();

			return personList;
		}
	}
}