using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public class RuleSetBagGroupPage : IGroupPageCreator<IRuleSetBag>
    {
        public IGroupPage CreateGroupPage(IEnumerable<IRuleSetBag> entityCollection, IGroupPageOptions groupPageOptions)
        {
            //Create the GroupPage object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) { DescriptionKey = groupPageOptions.CurrentGroupPageNameKey };

            var allPersonPeriods = (from p in groupPageOptions.Persons
                                   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
                                   select
                                       new
                                       {
                                           Person = p, pp.RuleSetBag
                                       }).ToList();

            var connectedRuleSets = new HashSet<IRuleSetBag>();
            foreach (var ppp in allPersonPeriods)
            {
                connectedRuleSets.Add(ppp.RuleSetBag);
            }

            foreach (IRuleSetBag ruleSetBag in entityCollection.OrderBy(c => c.Description.Name))
            {
                if (((IDeleteTag)ruleSetBag).IsDeleted) continue;

                //Create a root Group Object & add into GroupPage
                IRootPersonGroup rootGroup = new RootPersonGroup(ruleSetBag.Description.Name);
				if (!groupPage.IsUserDefined())
					rootGroup.SetId(ruleSetBag.Id);

                if(connectedRuleSets.Contains(ruleSetBag))
                {
                    // find all persons with rulesetbag
                    var bag = ruleSetBag;
                    foreach (var ppp in allPersonPeriods.Where(ppp => bag.Equals(ppp.RuleSetBag)).Select(ppp => ppp.Person).Distinct())
                    {
                        rootGroup.AddPerson(ppp);
                    }
                }

                //Add into GroupPage
                rootGroup.Name = rootGroup.Name;
                groupPage.AddRootPersonGroup(rootGroup);
            }
            return groupPage;
        }
    }
}