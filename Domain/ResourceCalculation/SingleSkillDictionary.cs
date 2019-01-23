using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface ISingleSkillDictionary
	{
		void Create(IList<IPerson> persons, DateOnlyPeriod period);
		bool IsSingleSkill(IPerson person, DateOnly dateOnly);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SingleSkillDictionary : ISingleSkillDictionary
	{
		private  IList<IPerson> _persons;
		private DateOnlyPeriod _period;
		private readonly IDictionary<KeyValuePair<IPerson, DateOnly>, bool> _dictionary = new Dictionary<KeyValuePair<IPerson, DateOnly>, bool>(); 
 
		public void Create(IList<IPerson> persons, DateOnlyPeriod period)
		{
			_persons = persons;
			_period = period;
			_dictionary.Clear();

			foreach (var date in _period.DayCollection())
			{
				IList<IPerson> singleSkilledPersons = new List<IPerson>(_persons);
				HashSet<ISkill> notSingleSkills = new HashSet<ISkill>();
				
				foreach (var person in _persons)
				{
					var personPeriod = person.Period(date);
					if(personPeriod == null) continue;

					var activePersonSkills = (from a in personPeriod.PersonSkillCollection
					                         where a.Active && !((IDeleteTag) a.Skill).IsDeleted
					                         select a).ToList();

					if (activePersonSkills.Count <= 1) continue;
					singleSkilledPersons.Remove(person);
					_dictionary.Add(new KeyValuePair<IPerson, DateOnly>(person, date), false);

					foreach (var personSkill in activePersonSkills)
					{
						if(!notSingleSkills.Contains(personSkill.Skill))
						{
							notSingleSkills.Add(personSkill.Skill);	
						}
					}
				}

				foreach (var singleSkilledPerson in singleSkilledPersons)
				{
					var personPeriod = singleSkilledPerson.Period(date);
					if (personPeriod == null) continue;
					
					var activePersonSkills = (from a in personPeriod.PersonSkillCollection
											  where a.Active && !((IDeleteTag)a.Skill).IsDeleted
											  select a).ToList();
					var isSingleSkill = true;
					foreach (var personSkill in activePersonSkills)
					{
						var skill = personSkill.Skill;
						var skillType = skill.SkillType as SkillTypePhone;

						if (skillType == null || notSingleSkills.Contains(skill))
						{
							isSingleSkill = false;
							break;
						}
					}
	
					_dictionary.Add(new KeyValuePair<IPerson, DateOnly>(singleSkilledPerson, date), isSingleSkill);		
				}
			}	
		}

		public bool IsSingleSkill(IPerson person, DateOnly dateOnly)
		{
			var kvp = new KeyValuePair<IPerson, DateOnly>(person, dateOnly);
			bool result;
			if (!_dictionary.TryGetValue(kvp, out result)) 
				return false;

			return result;
		}
	}
}
