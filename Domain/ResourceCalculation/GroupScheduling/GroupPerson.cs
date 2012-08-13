using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	// An Interface to be possible to Mock the creation of GroupPerson

	public sealed class GroupPerson : Person, IGroupPerson
    {
        private readonly IList<IPerson> _groupMembers = new List<IPerson>();
        private readonly DateOnly _dateOnly;

        public GroupPerson(IList<IPerson> persons, DateOnly dateOnly, string name)
        {
            _dateOnly = dateOnly;
            setGroupMembers(persons);
            setPersonSkillsOnDate();
            SetCommonRuleSetBagOnDate();
			SetId(Guid.NewGuid());
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
            // Add when needed don't forget mocks in test
            //PermissionInformation.SetCulture(persons[0].PermissionInformation.Culture());
            //PermissionInformation.SetUICulture(persons[0].PermissionInformation.UICulture());
            
                
            addPersonsToGroupMembers(persons);
        }

        private void addPersonsToGroupMembers(IEnumerable<IPerson> persons)
        {
            // TODO more checks on state on the date, 
            
            //must be in same TimeZone
            
            foreach (var person in persons)
            {
                if(checkSameTimeZone(person) && checkPersonPeriod(person))
                    _groupMembers.Add(person);
            }
        }

		private bool checkSameTimeZone(IPerson person)

		{
			var timeZone = PermissionInformation.DefaultTimeZone();
			return (person.PermissionInformation.DefaultTimeZone().Equals(timeZone));
		}

		private bool checkPersonPeriod(IPerson person)
		{
			return (person.Period(_dateOnly) != null);
		}

        public ReadOnlyCollection<IPerson> GroupMembers
        {
            get { return new ReadOnlyCollection<IPerson>(_groupMembers); }
        }

		public IPossibleStartEndCategory CommonPossibleStartEndCategory { get ; set ; }

		private void setPersonSkillsOnDate()
        {
            var listOfPersonSkills = new List<IPersonSkill>();

			foreach (IPerson person in _groupMembers)
			{
				var personPeriod = person.Period(_dateOnly);

				var pSkills = personPeriod.PersonSkillCollection;
				foreach (IPersonSkill personSkill in pSkills)
				{
					IPersonSkill foundPersonSkill = null;
					foreach (IPersonSkill combinedPersonSkill in listOfPersonSkills)
					{
						if (combinedPersonSkill.Skill.Equals(personSkill.Skill))
						{
							foundPersonSkill = combinedPersonSkill;
							break;
						}
					}
					if (foundPersonSkill != null)
					{
						foundPersonSkill.SkillPercentage =
							new Percent(personSkill.SkillPercentage.Value + foundPersonSkill.SkillPercentage.Value);
					}
					else
					{
						listOfPersonSkills.Add((PersonSkill)personSkill.Clone());
					}
				}
			}
        	foreach (var personSkill in listOfPersonSkills)
            {
                PersonPeriodCollection[0].AddPersonSkill(personSkill);
            }
        }

        private void SetCommonRuleSetBagOnDate()
        {
            IRuleSetBag returnBag = null;

			foreach (var person in _groupMembers)
			{
				var personPeriod = person.Period(_dateOnly);
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
							foreach (var workShiftRuleSet in returnBag.RuleSetCollection)
							{
								if (workShiftRuleSet.TemplateGenerator.Category.Equals(category))
									ruleSetsToRemove.Add(workShiftRuleSet);
							}
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
