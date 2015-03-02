using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    /// <summary>
    /// Pouplates  PartTimePercentage objects for a GroupPage 
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/7/2008
    /// </remarks>
    public class PartTimePercentageGroupPage : IGroupPageCreator<IPartTimePercentage>
    {
        public IGroupPage CreateGroupPage(IEnumerable<IPartTimePercentage> entityCollection, IGroupPageOptions groupPageOptions)
        {
            //Create the GroupPage object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) { DescriptionKey = groupPageOptions.CurrentGroupPageNameKey };

            var allPersonPeriods = (from p in groupPageOptions.Persons
                                   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
                                   select
                                       new
                                       {
                                           Person = p,
                                           pp.PersonContract.PartTimePercentage
                                       }).ToList();

            var connectedPartTimePercentages = new HashSet<IPartTimePercentage>();
            foreach (var ppp in allPersonPeriods)
            {
                connectedPartTimePercentages.Add(ppp.PartTimePercentage);
            }

            foreach (IPartTimePercentage partTimePercentage in entityCollection.OrderBy(c => c.Description.Name))
            {
                if (((IDeleteTag)partTimePercentage).IsDeleted) continue;

                //Create a root Group Object & add into GroupPage
                IRootPersonGroup rootGroup = new RootPersonGroup(partTimePercentage.Description.Name);
            	if (!groupPage.IsUserDefined())
            		rootGroup.SetId(partTimePercentage.Id);

            	if (connectedPartTimePercentages.Contains(partTimePercentage))
                {
                    var percentage = partTimePercentage;
                    var personPeriodsWithPartTimePercentage =
                        allPersonPeriods.Where(
                            p => percentage.Equals(p.PartTimePercentage));
                    var personsWithPartTimePercentage =
                        personPeriodsWithPartTimePercentage.Select(pp => pp.Person).Distinct();
                    foreach (var personWithPartTimePercentage in personsWithPartTimePercentage)
                    {
                        rootGroup.AddPerson(personWithPartTimePercentage);
                    }
                }

                //Add into GroupPage
                groupPage.AddRootPersonGroup(rootGroup);
            }
            return groupPage;
        }
    }
}