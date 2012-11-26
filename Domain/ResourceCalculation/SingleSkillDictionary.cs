﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillDictionary
	{
		void Create();
		bool IsSingleSkill(IPerson person, DateOnly dateOnly);
	}

	public class SingleSkillDictionary :ISingleSkillDictionary
	{
		private readonly IList<IPerson> _persons;
		private DateOnlyPeriod _period;
		private readonly IDictionary<KeyValuePair<IPerson, DateOnly>, bool> _dictionary; 
 
		public SingleSkillDictionary(IList<IPerson> persons, DateOnlyPeriod period)
		{
			_dictionary = new Dictionary<KeyValuePair<IPerson, DateOnly>, bool>();
			_persons = persons;
			_period = period;
		}

		public void Create()
		{
			foreach (var date in _period.DayCollection())
			{
				IList<IPerson> singleSkilledPersons = new List<IPerson>(_persons);
				IList<ISkill> notSingleSkills = new List<ISkill>();
				
				foreach (var person in _persons)
				{
					var personPeriod = person.Period(date);

					if(personPeriod.PersonSkillCollection.Count > 1)
					{
						singleSkilledPersons.Remove(person);
						_dictionary.Add(new KeyValuePair<IPerson, DateOnly>(person, date), false);

						foreach (var personSkill in personPeriod.PersonSkillCollection)
						{
							if(!notSingleSkills.Contains(personSkill.Skill))
							{
								notSingleSkills.Add(personSkill.Skill);	
							}
						}
					}
				}

				foreach (var singleSkilledPerson in singleSkilledPersons)
				{
					var personPeriod = singleSkilledPerson.Period(date);
					var isSingleSkill = true;
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						if(notSingleSkills.Contains(personSkill.Skill))
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
			if (!_dictionary.ContainsKey(kvp)) 
				return false;

			return _dictionary[kvp];
		}
	}
}
