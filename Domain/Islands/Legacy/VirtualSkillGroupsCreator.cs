﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	public interface IVirtualSkillGroupsCreator
	{
		VirtualSkillGroupsCreatorResult GroupOnDate(DateOnly dateOnly, IEnumerable<IPerson> personList);
	}

	public class VirtualSkillGroupsCreator : IVirtualSkillGroupsCreator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public VirtualSkillGroupsCreator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public VirtualSkillGroupsCreatorResult GroupOnDate(DateOnly dateOnly, IEnumerable<IPerson> personList)
		{
			var skillGroupKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
			var skillKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
			var skillGroupKeyUniqueNameDic = new Dictionary<string, string>();
			var personSkillGroupDic = new Dictionary<IPerson, string>();

			foreach (var person in personList)
			{
				addPerson(person, dateOnly, skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic, personSkillGroupDic);
			}

			return new VirtualSkillGroupsCreatorResult(skillGroupKeyPersonListDic, skillKeyPersonListDic, skillGroupKeyUniqueNameDic, personSkillGroupDic);
		}

		private void addPerson(IPerson person, DateOnly date, IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic,
			IDictionary<IPerson, string> personSkillGroupDic)
		{
			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return;

			var key = string.Empty;
			foreach (var personSkill in _personalSkillsProvider.PersonSkills(personPeriod).OrderBy(s => s.Skill.Id))
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

	public class VirtualSkillGroupsCreatorResult : ISkillGroupInfo
	{
		private readonly IDictionary<string, IList<IPerson>> _skillGroupKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, IList<IPerson>> _skillKeyPersonListDic = new Dictionary<string, IList<IPerson>>();
		private readonly IDictionary<string, string> _skillGroupKeyUniqueNameDic = new Dictionary<string, string>();
		private readonly IDictionary<IPerson, string> _personSkillGroupDic;

		public VirtualSkillGroupsCreatorResult(IDictionary<string, IList<IPerson>> skillGroupKeyPersonListDic,
			IDictionary<string, IList<IPerson>> skillKeyPersonListDic, IDictionary<string, string> skillGroupKeyUniqueNameDic,
			IDictionary<IPerson, string> personSkillGroupDic)
		{
			_skillGroupKeyPersonListDic = skillGroupKeyPersonListDic;
			_skillKeyPersonListDic = skillKeyPersonListDic;
			_skillGroupKeyUniqueNameDic = skillGroupKeyUniqueNameDic;
			_personSkillGroupDic = personSkillGroupDic;
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

		public IEnumerable<IEnumerable<IPerson>> AgentsGroupedBySkillGroup()
		{
			return GetKeys().Select(GetPersonsForSkillGroupKey);
		}

		public int NumberOfAgentsInSameSkillGroup(IPerson person)
		{
			string key;
			return _personSkillGroupDic.TryGetValue(person, out key) ?
				GetPersonsForSkillGroupKey(key).Count() : 
				int.MaxValue;
		}
	}

	public class VirtualSkillGroupsCreatorThatThrows : IVirtualSkillGroupsCreator
	{
		public VirtualSkillGroupsCreatorResult GroupOnDate(DateOnly dateOnly, IEnumerable<IPerson> personList)
		{
			throw new System.NotImplementedException();
		}
	}
}