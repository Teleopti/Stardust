using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
    public interface INonBlendPersonSkillFromGroupingCreator
    {
        void ProcessPersons(ISkill skill, IList<IPerson> persons, DateOnly dateOnly);
    }

    public class NonBlendPersonSkillFromGroupingCreator : INonBlendPersonSkillFromGroupingCreator
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void ProcessPersons(ISkill skill, IList<IPerson> persons, DateOnly dateOnly)
        {
            foreach (var person in persons)
            {
                ProcessPerson(skill, person, dateOnly);
            }
        }

        private static void ProcessPerson(ISkill skill, IPerson person, DateOnly dateOnly)
        {
            var personPeriod = person.Period(dateOnly);
            if(personPeriod != null)
            {
                personPeriod.PersonNonBlendSkillCollection.Clear();
                personPeriod.AddPersonNonBlendSkill(new PersonSkill(skill, new Percent(1)));
            }
        }
    }
}