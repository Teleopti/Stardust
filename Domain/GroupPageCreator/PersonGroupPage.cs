using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    /// <summary>
    /// Creates a GroupPage with RootPersonGroups according to the organization structure
    /// </summary>
    public class PersonGroupPage : IGroupPageCreator<IBusinessUnit>
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IGroupPage CreateGroupPage(IEnumerable<IBusinessUnit> entityCollection, IGroupPageOptions groupPageOptions)
        {
            //Creates the GroupPage object
            var businessUnit = entityCollection.Single();
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName)
                                       {
                                           DescriptionKey = groupPageOptions.CurrentGroupPageNameKey,
                                           RootNodeName = businessUnit.Name
                                       };

            //First users only
            PersonIsUserSpecification isUserSpecification = new PersonIsUserSpecification(groupPageOptions.SelectedPeriod.EndDate);
            IRootPersonGroup userGroup = new RootPersonGroup(UserTexts.Resources.NotInHierarchy);
            groupPageOptions.Persons.FilterBySpecification(isUserSpecification).ForEach(userGroup.AddPerson);
            groupPage.AddRootPersonGroup(userGroup);

            //Creates a root Group object & add into GroupPage
            var allPersonPeriods = (from p in groupPageOptions.Persons
                                   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
								   let team = pp.Team
                                   select
                                       new
                                       {
                                           Person = p,
                                           PersonPeriod = pp,
										   Team = team,
                                       }).ToLookup(period => period.Team);
            foreach (var site in businessUnit.SiteCollection.OrderBy(s => s.Description.Name))
            {
                if (((IDeleteTag) site).IsDeleted) continue;

                IRootPersonGroup rootGroup = new RootPersonGroup(site.Description.Name);
                rootGroup.Entity = site;
                groupPage.AddRootPersonGroup(rootGroup);

                foreach (var team in site.TeamCollection.OrderBy(t => t.Description.Name))
                {
                    if (!team.IsChoosable) continue;

                    IChildPersonGroup teamGroup = new ChildPersonGroup(team.Description.Name);
                    teamGroup.Entity = team;
                    rootGroup.AddChildGroup(teamGroup);

                    var personPeriodsWithTeam = allPersonPeriods[team];
                    var teamPersons = personPeriodsWithTeam.Select(pp => pp.Person).Distinct();
                    foreach (var teamPerson in teamPersons)
                    {
                        teamGroup.AddPerson(teamPerson);
                    }
                }
            }

            return groupPage;
        }
    }
}