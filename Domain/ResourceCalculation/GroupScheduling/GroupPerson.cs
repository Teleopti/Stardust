using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public sealed class GroupPerson : Person, IGroupPerson
    {
        private readonly IDictionary<IPerson, IPersonPeriod> _groupMembers = new Dictionary<IPerson,IPersonPeriod>();
        private readonly DateOnly _dateOnly;

        public GroupPerson(IList<IPerson> persons, DateOnly dateOnly, string name, Guid? guid)
        {
            _dateOnly = dateOnly;
            setGroupMembers(persons);
            setPersonSkillsOnDate();
            setCommonRuleSetBagOnDate();
			SetId(guid);
			Name = new Name(name,"");
        }

		private void setGroupMembers(IList<IPerson> persons)
        {
            if(persons.IsEmpty())
                throw new ArgumentOutOfRangeException("persons","The list of persons must not be empty");
            // make sure we have a PersonPeriod
            if (PersonPeriodCollection.Count == 0)
                AddPersonPeriod(new PersonPeriod(_dateOnly,
                    new PersonContract(new Contract("contract"),
                        new PartTimePercentage("percentage"),
                        new ContractSchedule("contractschedule")),
                        new Team()));

            if (PersonSchedulePeriodCollection.Count == 0)
                // make sure we have a SchedulePeriod
                AddSchedulePeriod(new SchedulePeriod(_dateOnly, SchedulePeriodType.Day, 1));
            
            PermissionInformation.SetDefaultTimeZone(persons[0].PermissionInformation.DefaultTimeZone());

            addPersonsToGroupMembers(persons);
        }

        private void addPersonsToGroupMembers(IEnumerable<IPerson> persons)
        {
            foreach (var person in persons)
            {
                if (checkSameTimeZone(person))
                {
	                var personPeriod = person.Period(_dateOnly);
					if (personPeriod != null)
					{
						_groupMembers.Add(person,personPeriod);
					}
                }
            }
        }

		private bool checkSameTimeZone(IPerson person)
		{
			var timeZone = PermissionInformation.DefaultTimeZone();
			return (person.PermissionInformation.DefaultTimeZone().Equals(timeZone));
		}

        public IEnumerable<IPerson> GroupMembers
        {
            get { return _groupMembers.Keys; }
        }

		private void setPersonSkillsOnDate()
		{
			var personPeriod = PersonPeriodCollection[0];

			foreach (var person in _groupMembers)
			{
				var pSkills = person.Value.PersonSkillCollection;
				foreach (IPersonSkill personSkill in pSkills)
				{
					IPersonSkill foundPersonSkill = personPeriod.PersonSkillCollection.FirstOrDefault(combinedPersonSkill => combinedPersonSkill.Active && combinedPersonSkill.Skill.Equals(personSkill.Skill));
					if (foundPersonSkill != null)
					{
						ChangeSkillProficiency(personSkill.Skill, new Percent(personSkill.SkillPercentage.Value + foundPersonSkill.SkillPercentage.Value), personPeriod);
					}
					else
					{
						AddSkill((IPersonSkill) personSkill.Clone(), personPeriod);
					}
				}
			}
        }

        private void setCommonRuleSetBagOnDate()
        {
            IRuleSetBag returnBag = null;

			foreach (var person in _groupMembers)
			{
				var personPeriod = person.Value;
				var bag = personPeriod.RuleSetBag;

				if (bag == null)
				{
					returnBag = new RuleSetBagForGroupPerson();
					break;
				}

				if (returnBag == null)
				{
					returnBag = new RuleSetBagForGroupPerson();
					foreach (var workShiftRuleSet in bag.RuleSetCollection)
					{
						returnBag.AddRuleSet(workShiftRuleSet);
					}
				}
				else
				{
					//remove rulesets on category we don't have in this bag
					var ruleSetsToRemove = new List<IWorkShiftRuleSet>();
					foreach (var category in returnBag.ShiftCategoriesInBag())
					{
						//if we don't have the category in this bag we must remove all rulesets with that bag
						if (!bag.ShiftCategoriesInBag().Contains(category))
						{
							ruleSetsToRemove.AddRange(returnBag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.TemplateGenerator.Category.Equals(category)));
						}
					}
					foreach (var workShiftRuleSet in ruleSetsToRemove)
					{
						returnBag.RemoveRuleSet(workShiftRuleSet);
					}
					//add rulesets we don't have but on category we have
					foreach (var workShiftRuleSet in bag.RuleSetCollection)
					{
						if (!returnBag.RuleSetCollection.Contains(workShiftRuleSet) &&
						    returnBag.ShiftCategoriesInBag().Contains(workShiftRuleSet.TemplateGenerator.Category))
							returnBag.AddRuleSet(workShiftRuleSet);
					}
				}
			}

        	PersonPeriodCollection[0].RuleSetBag = returnBag;            
        }
    }
}
