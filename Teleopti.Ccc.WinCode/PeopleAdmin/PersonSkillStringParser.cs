using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	public class PersonSkillStringParser
	{
		private readonly IList<IPersonSkill> _personSkills;

		public PersonSkillStringParser(IList<IPersonSkill> personSkills)
		{
			_personSkills = personSkills;
		}

		public IList<IPersonSkill> Parse(string value)
		{
			IList<IPersonSkill> retPersonSkill = new List<IPersonSkill>();
			if(!string.IsNullOrEmpty(value))
			{
				Char[] separator = { ',' };
				string[] personSkillCollection = value.Split(separator);


				IList<ISkill> addedSkills = new List<ISkill>();
				foreach (string t in personSkillCollection)
				{
					foreach (IPersonSkill t1 in _personSkills)
					{
						if (t1.Skill.Name.Trim() == t.Trim() && !addedSkills.Contains(t1.Skill))
						{
							addedSkills.Add(t1.Skill);
							retPersonSkill.Add(new PersonSkill(t1.Skill, t1.SkillPercentage));
							break;
						}
					}
				}
			}
			
			return retPersonSkill;
		}
	}
}