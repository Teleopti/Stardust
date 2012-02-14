using Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an old entity of Grouping to a new entity of GroupPage
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/4/2008
    /// </remarks>
    public class GroupingMapper : Mapper<IGroupPage, global::Domain.Grouping>
    {
        // length of the Name column
        private const int MaxLengthName = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        public GroupingMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
        }

        /// <summary>
        /// Maps the specified old entity to the new entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IGroupPage Map(Grouping oldEntity)
        {
            string groupingName = ConversionHelper.MapString(oldEntity.Name, MaxLengthName);
            IGroupPage newGroupPage = new GroupPage(groupingName);

            foreach (global::Domain.Group topGroup in oldEntity.TopLevelGroups)
            {
                string rootGroupName = ConversionHelper.MapString(topGroup.Name, MaxLengthName);
                RootPersonGroup rootGroup = new RootPersonGroup(rootGroupName);

                // builds the Person collection of the rootGroup
                BuildPersonCollection(rootGroup, topGroup);

                // each top level group may have a hierarchy of child groups
                BuildChildHierarchy(rootGroup, topGroup);

                // adds this toplevel group to the GroupPage object has been mapped
                newGroupPage.AddRootPersonGroup(rootGroup);
            }

            return newGroupPage;

        }

        /// <summary>
        /// Builds the child hierarchy.
        /// </summary>
        /// <param name="topLevelGroup">The top level group.</param>
        /// <param name="topGroup">The top group.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/4/2008
        /// </remarks>
        private void BuildChildHierarchy(PersonGroupBase topLevelGroup, global::Domain.Group topGroup)
        {
            if (topGroup.Childs != null)
            {
                foreach (global::Domain.Group oldChildEntity in topGroup.Childs)
                {
                    string childGroupName = ConversionHelper.MapString(oldChildEntity.Name, MaxLengthName);
                    // maps Group to a ChildPersonGroup
                    ChildPersonGroup childGroup = new ChildPersonGroup(childGroupName);

                    // builds the person collection of childGroup
                    BuildPersonCollection(childGroup, oldChildEntity);

                    topLevelGroup.AddChildGroup(childGroup);

                    // this ChildPersonGroup may have a child collection
                    BuildChildHierarchy(childGroup, oldChildEntity);
                }
            }
        }


        /// <summary>
        /// Builds the person collection.
        /// </summary>
        /// <param name="personGroupBase">The person group base.</param>
        /// <param name="group">The group.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/4/2008
        /// </remarks>
        private void BuildPersonCollection(PersonGroupBase personGroupBase, global::Domain.Group group)
        {
            foreach (global::Domain.Agent agent in group.Agents)
            {
                IPerson person = MappedObjectPair.Agent.GetPaired(agent);

                if (person != null)
                {
                    personGroupBase.AddPerson(person);
                }
            }
        }
    }
}
