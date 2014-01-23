using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    /// <summary>
    /// Creates a GroupPage with RootPersonGroups which have persons with same notes
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/8/2008
    /// </remarks>
    public class PersonNoteGroupPage : IGroupPageCreator<IPerson>
    {
        public IGroupPage CreateGroupPage(IEnumerable<IPerson> entityCollection, IGroupPageOptions groupPageOptions)
        {
            //Loads all agents, i.e persons who has a team.
            var personGroups =
                groupPageOptions.Persons.Where(person => !string.IsNullOrEmpty(person.Note) &&
                                                         person.PersonPeriods(groupPageOptions.SelectedPeriod).Count > 0)
                    .GroupBy(person => person.Note, StringComparer.Ordinal);

            //Creates the GroupPage object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) {DescriptionKey = groupPageOptions.CurrentGroupPageNameKey};

            foreach (var personGroup in personGroups)
            {
                //Creates a root Group object & add into GroupPage
                string personGrpDesc = personGroup.Key.Length > 50 ? personGroup.Key.Substring(0, 48) + ".." : personGroup.Key;
                IRootPersonGroup rootGroup = new RootPersonGroup(personGrpDesc);
				if (!groupPage.IsUserDefined())
					rootGroup.SetId(Guid.NewGuid());
                foreach (var p in personGroup)
                {
                    rootGroup.AddPerson(p);
                }

                //Add into GroupPage
                groupPage.AddRootPersonGroup(rootGroup);
            }

            return groupPage;
        }
    }
}