using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public class SingleAgentTeamGroupPage : IGroupPageCreator<IPerson>
    {
        public IGroupPage CreateGroupPage(IEnumerable<IPerson> entityCollection, IGroupPageOptions groupPageOptions)
        {
            if (entityCollection == null) return null;
            if (groupPageOptions == null) return null;
            //Create the GroupPage object
            IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) { DescriptionKey = groupPageOptions.CurrentGroupPageNameKey };

            foreach (IPerson person in entityCollection)
            {
                //Create a root Group Object & add into GroupPage
	            var descriptionName = person.Name.FirstName + "-" + person.Name.LastName;
	            if (descriptionName.Length > 50)
		            descriptionName = descriptionName.Substring(0, 50);
                IRootPersonGroup rootGroup = new RootPersonGroup(descriptionName);
                if (!groupPage.IsUserDefined())
                    rootGroup.SetId(person.Id);

                rootGroup.AddPerson(person);
                
                //Add into GroupPage
                groupPage.AddRootPersonGroup(rootGroup);
            }
            return groupPage;
        }
    }
}