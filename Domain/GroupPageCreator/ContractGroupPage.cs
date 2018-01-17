using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    /// <summary>
    /// Return GroupPage base on contracts.
    /// </summary>
    public class ContractGroupPage : IGroupPageCreator<IContract>
    {
        public IGroupPage CreateGroupPage(IEnumerable<IContract> entityCollection,IGroupPageOptions groupPageOptions)
        {
            //Create the GroupPage object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) {DescriptionKey = groupPageOptions.CurrentGroupPageNameKey};

            var allPersonPeriods = (from p in groupPageOptions.Persons
                                   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
                                   select
                                       new
                                           {
                                               Person = p,
                                               pp.PersonContract.Contract
                                           }).ToLookup(k => k.Contract);
			
            foreach (IContract contract in entityCollection.OrderBy(c => c.Description.Name))
            {
                if (((IDeleteTag)contract).IsDeleted) continue;

                //Create a root Group Object & add into GroupPage
                IRootPersonGroup rootGroup = new RootPersonGroup(contract.Description.Name);
				if (!groupPage.IsUserDefined())
					rootGroup.SetId(contract.Id);

                if (allPersonPeriods[contract].Any())
                {
                    var personsWithContract = allPersonPeriods[contract].Select(pp => pp.Person).Distinct();
                    foreach (var personWithContract in personsWithContract)
                    {
                        rootGroup.AddPerson(personWithContract);
                    }
                }

                //Add into GroupPage
                groupPage.AddRootPersonGroup(rootGroup);
            }
            return groupPage;
        }
    }
}