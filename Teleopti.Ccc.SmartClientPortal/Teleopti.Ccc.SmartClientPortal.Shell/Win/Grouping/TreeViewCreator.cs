using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
    /// <summary>
    /// Creates the tree items.
    /// </summary>
    public class TreeViewCreator
    {
        private readonly IGroupPageHelper _groupPageDataProvider;
        private IList<IPerson> _remainingPersonList;
        private ICommonNameDescriptionSetting _commonNameDescription;

        public TreeViewCreator(IGroupPageHelper groupPageDataProvider)
        {
            _groupPageDataProvider = groupPageDataProvider;
        }

        /// <summary>
        /// Creates the view.
        /// </summary>
        /// <param name="groupPage">The group page.</param>
        /// <param name="applicationFunction">The application function.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-25
        /// </remarks>
        public IList<TreeNodeAdv> CreateView(IGroupPage groupPage, IApplicationFunction applicationFunction)
        {
            LoadSettings();
            IEnumerable<IPerson> peopleList = _groupPageDataProvider.PersonCollection;
            _remainingPersonList = (peopleList != null) ? new List<IPerson>(peopleList) : new List<IPerson>();
            IList<TreeNodeAdv> nodes = new List<TreeNodeAdv>();
            var authorization = PrincipalAuthorization.Current_DONTUSE();
            string functionPath = applicationFunction.FunctionPath;

            try
            {
                foreach (IRootPersonGroup rootGroup in groupPage.RootGroupCollection)
                {
	                var node = new TreeNodeAdv(rootGroup.Name)
	                {
		                TagObject = rootGroup,
		                Tag = GroupingConstants.NodeTypeGroup
	                };
	                var imageIndex = 2;
                    if (rootGroup.IsSite)
                    {
                        imageIndex = 1;
                    }
                    node.LeftImageIndices = new[] { imageIndex };
                    nodes.Add(node);

                    //Recursively Populate from root
                    RecursivelyLoadTree(rootGroup.ChildGroupCollection, node, applicationFunction);

                    //add persons to corresponding node
                    AddPerson(node, rootGroup.PersonCollection, applicationFunction);
                }

                //Get remaining persons who are not added to any group.
                //Add persons not in group to the main root.
                var period = _groupPageDataProvider.SelectedPeriod;
                
                foreach (IPerson person in _remainingPersonList)
                {
                    //Check whether person is permitted
                    if (authorization.IsPermitted(functionPath, period.StartDate, person) || authorization.IsPermitted(functionPath, period.EndDate, person))
                    {
                        TreeNodeAdv personNode =
                            new TreeNodeAdv(_commonNameDescription.BuildFor(person));
                        personNode.TagObject = person;
                        personNode.Tag = GroupingConstants.NodeTypePerson;
                        personNode.LeftImageIndices = new[] {3};
                        nodes.Add(personNode);
                    }
                }
            }
            finally
            {
            }
            return nodes;
        }

        /// <summary>
        /// Recursivelies the load tree.
        /// </summary>
        /// <param name="childGroupCollection">The child group collection.</param>
        /// <param name="node">The node.</param>
        /// <param name="applicationFunction">The application function.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-26
        /// </remarks>
        private void RecursivelyLoadTree(IEnumerable<IChildPersonGroup> childGroupCollection, TreeNodeAdv node, IApplicationFunction applicationFunction)
        {
            foreach (IChildPersonGroup childGroup in childGroupCollection)
            {
                string nodeName = childGroup.Name;
                TreeNodeAdv childNode = new TreeNodeAdv(nodeName);
                childNode.TagObject = childGroup;
                childNode.Tag = GroupingConstants.NodeTypeGroup;
                var imageIndex = 2;
                if (childGroup.IsSite)
                {
                    imageIndex = 1;
                }
                childNode.LeftImageIndices = new[] { imageIndex };
                node.Nodes.Add(childNode);

                //Repeat for each child
                RecursivelyLoadTree(childGroup.ChildGroupCollection, childNode, applicationFunction);

                //add persons to corresponding node
                AddPerson(childNode, childGroup.PersonCollection, applicationFunction);
            }
        }

        /// <summary>
        /// Adds the person.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="applicationFunction">The application function.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-26
        /// </remarks>
        private void AddPerson(TreeNodeAdv node, IEnumerable<IPerson> personCollection, IApplicationFunction applicationFunction)
        {
            var period = _groupPageDataProvider.SelectedPeriod;
            var authorization = PrincipalAuthorization.Current_DONTUSE();
            string functionPath = applicationFunction.FunctionPath;

            //add persons to corresponding node
            foreach (var person in personCollection.Select(p=>new {Person = p, Description = _commonNameDescription.BuildFor(p)}).OrderBy(p => p.Description))
            {
                IDeleteTag maybeDeletedPerson = (IDeleteTag) person.Person;
                if (maybeDeletedPerson.IsDeleted)
                {
                    continue;
                }

                //Check whether person is permitted
                if (authorization.IsPermitted(functionPath, period.StartDate, person.Person) || authorization.IsPermitted(functionPath, period.EndDate, person.Person))
                {
                    TreeNodeAdv personNode = new TreeNodeAdv(person.Description);
                    personNode.TagObject = person.Person;
                    personNode.Tag = GroupingConstants.NodeTypePerson;
                    personNode.LeftImageIndices = new[] { 3 };
                    node.Nodes.Add(personNode);

                    //remove added person from remaining PersonList
                    _remainingPersonList.Remove(person.Person);
                }
            }
        }

        public void LoadSettings()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISettingDataRepository settingDataRepository = GlobalSettingDataRepository.DONT_USE_CTOR(uow);

                _commonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
            }
        }
    }
}
