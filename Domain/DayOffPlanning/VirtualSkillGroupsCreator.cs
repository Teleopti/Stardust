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
			var personSkillGroupDic = new Dictionary<IPerson, string>();
			var allInvolvedSkills = new HashSet<ISkill>();

			foreach (var person in personList)
			{
				addPerson(person, dateOnly, skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic, personSkillGroupDic, allInvolvedSkills);
			}

			return new VirtualSkillGroupsCreatorResult(skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic, personSkillGroupDic, allInvolvedSkills);
		}

		private static void addPerson(IPerson person, DateOnly date, IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic,
			IDictionary<IPerson, string> personSkillGroupDic, ISet<ISkill> allSkills)
		{
			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return;

			var key = string.Empty;
			foreach (var personSkill in personPeriod.PersonSkillCollection.OrderBy(s => s.Skill.Id))
			{
				if (personSkill.Active && !((IDeleteTag) personSkill.Skill).IsDeleted)
				{
					var skillId = personSkill.Skill.Id;
					var thisId = skillId.HasValue ? 
						skillId.ToString() : 
						personSkill.Skill.GetHashCode().ToString();
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
					allSkills.Add(personSkill.Skill);
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

			personSkillGroupDic.Add(person, key);
		}
	}

	public class VirtualSkillGroupsCreatorResult
	{
		private readonly IDictionary<string, IList<IPerson>> _skillGroupKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, IList<IPerson>> _skillKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, string> _skillGroupKeyUniqueNameDic = new Dictionary<string, string>();
		private readonly IDictionary<IPerson, string> _personSkillGroupDic;
		private readonly IEnumerable<ISkill> _allInvolvedSkills;

		public VirtualSkillGroupsCreatorResult(IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic,
			IDictionary<IPerson, string> personSkillGroupDic, IEnumerable<ISkill> allInvolvedSkills)
		{
			_skillGroupKeyPersonListDic = skillGroupKeyPersonListDic;
			_skillKeyPersonListDic = skillKeyPersonListDic;
			_skillGroupKeyUniqueNameDic = skillGroupKeyUniqueNameDic;
			_personSkillGroupDic = personSkillGroupDic;
			_allInvolvedSkills = allInvolvedSkills;
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

		public IEnumerable<IEnumerable<IPerson>> GetSkillGroupTree()
		{
			return GetKeys().Select(GetPersonsForSkillGroupKey);
		}

		public int GetNumberOfAgentsInSkillGroupFromPerson(IPerson person)
		{
			string key;
			return _personSkillGroupDic.TryGetValue(person, out key) ?
				GetPersonsForSkillGroupKey(key).Count() : 
				int.MaxValue;
		}

		//TODO: not correct ATM
		public IEnumerable<ISkill> SkillsInSameGroupAs(ISkill skill)
		{
			var ret = new List<ISkill>();
			foreach (var key in _skillGroupKeyUniqueNameDic.Keys)
			{
				if (key.Contains(skill.Id.Value.ToString()))
				{
					var ids = key.Split('|');
					ret.AddRange(_allInvolvedSkills.Where(x => ids.Contains(x.Id.Value.ToString())));
				}
			}
			return ret;
		}
	}
}