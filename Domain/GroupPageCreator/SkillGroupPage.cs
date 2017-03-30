using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public class SkillGroupPage : IGroupPageCreator<ISkill>
    {
        public IGroupPage CreateGroupPage(IEnumerable<ISkill> entityCollection, IGroupPageOptions groupPageOptions)
        {
            //Create the Root object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) { DescriptionKey = groupPageOptions.CurrentGroupPageNameKey };

            var allPersonPeriods = (from p in groupPageOptions.Persons
                                   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
                                   select
                                       new
                                       {
                                           Person = p,
                                           pp.PersonSkillCollection
                                       }).ToList();

            foreach (ISkill skill in entityCollection.OrderBy(c => c.Name))
            {
                if (((IDeleteTag)skill).IsDeleted) continue;

                //Create a parent Group Object & add into GroupPage
                IRootPersonGroup rootGroup = new RootPersonGroup(skill.Name);
				if (!groupPage.IsUserDefined())
					rootGroup.SetId(skill.Id);

                var personsWithSkill = new HashSet<IPerson>();

                foreach (var personPeriod in allPersonPeriods)
                {
                    foreach (IPersonSkill personSkill in personPeriod.PersonSkillCollection.Where(s => s.Active))
                    {
                        if(personSkill.Skill.Equals(skill))
                        {
                            personsWithSkill.Add(personPeriod.Person);
                            break;
                        }
                    }
                }
                
                foreach (IPerson person in personsWithSkill)
                {
                    rootGroup.AddPerson(person);
                }

                if(rootGroup.PersonCollection.Count > 0)
                    groupPage.AddRootPersonGroup(rootGroup);
            }

            return groupPage;
        }
    }
}
