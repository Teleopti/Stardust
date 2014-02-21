using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
{
    public interface ILoadUserDefinedTabsCommand : IExecutableCommand, ILoadGroupPageCommand
    {
        Guid Id { get; }
    }

    public class LoadUserDefinedTabsCommand : ILoadUserDefinedTabsCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
	    private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
        private readonly IPersonSelectorView _personSelectorView;
        private readonly Guid _value;
        private readonly ICommonNameDescriptionSetting _commonAgentNameSettings;
        private readonly IApplicationFunction _applicationFunction;
        
        public LoadUserDefinedTabsCommand(IUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository,
            IPersonSelectorView personSelectorView, Guid value, ICommonNameDescriptionSetting commonAgentNameSettings, 
            IApplicationFunction applicationFunction)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
	        _personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
            _personSelectorView = personSelectorView;
            _value = value;
            _commonAgentNameSettings = commonAgentNameSettings;
            _applicationFunction = applicationFunction;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Execute()
        {
            var date = _personSelectorView.SelectedDate;
            IList<IPersonSelectorUserDefined> toNodes;
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                toNodes = _personSelectorReadOnlyRepository.GetUserDefinedTab(date, _value);
            }
            
            // r�ttigheter
            var auth = PrincipalAuthorization.Instance();
            var toRemove = new List<IPersonSelectorUserDefined>();
			if (_personSelectorView.VisiblePersonIds != null)
			{
				foreach (var toNode in toNodes)
				{
					if (!toNode.PersonId.Equals(new Guid()) && !_personSelectorView.VisiblePersonIds.Contains(toNode.PersonId))
						toRemove.Add(toNode);

				}
			}
			foreach (var personSelectorOrganization in toRemove)
			{
				toNodes.Remove(personSelectorOrganization);
			}
            foreach (var toNode in toNodes)
            {
                if (toNode.PersonId != new Guid())
                {
                    if (_applicationFunction != null)
                    {
                        if (!auth.IsPermitted(_applicationFunction.FunctionPath, date, toNode))
                            toRemove.Add(toNode);
                    }
                }
            }
            foreach (var personSelectorOrganization in toRemove)
            {
                toNodes.Remove(personSelectorOrganization);
            }

			var nodes = new List<TreeNodeAdv>();
            var root = new TreeNodeAdv("") { LeftImageIndices = new[] { 1 }, Expanded = true ,Tag = new List<Guid>()};
            nodes.Add(root);
            var currNode = new TreeNodeAdv("") { TagObject = new Guid()};

            var nodeDictionary = new Dictionary<Guid, TreeNodeAdv>();
            foreach (var personSelectorUserDefined in toNodes)
            {
                TreeNodeAdv tryNode;
                //the parent has to come before the child otherwise we got problems in this implementation
                if (!personSelectorUserDefined.ParentId.HasValue && !nodeDictionary.TryGetValue(personSelectorUserDefined.NodeId, out tryNode))
                {
                    root.Text = personSelectorUserDefined.Node;
                    root.TagObject = personSelectorUserDefined.NodeId;
                    nodeDictionary.Add(personSelectorUserDefined.NodeId, root);
                }
                else
                {
                    
                    if ((Guid)currNode.TagObject != personSelectorUserDefined.NodeId && !nodeDictionary.TryGetValue(personSelectorUserDefined.NodeId, out tryNode))
                    {
                        currNode = new TreeNodeAdv(personSelectorUserDefined.Node)
                                       {
                                           LeftImageIndices = new[] { 2 }, TagObject = personSelectorUserDefined.NodeId,
                                           Tag = new List<Guid>()
                                       };
                        nodeDictionary.Add(personSelectorUserDefined.NodeId, currNode);
                        TreeNodeAdv parent;
                        if (nodeDictionary.TryGetValue(personSelectorUserDefined.ParentId.Value, out parent))
                        {
                            if (!parent.Equals(root))
                                parent.Nodes.Insert(0, currNode);
                            else
                                parent.Nodes.Add(currNode);
                        }
                    }    
                }
                
                // and here have a list with one guid
                if (personSelectorUserDefined.Show && !personSelectorUserDefined.PersonId.Equals(new Guid()))
                {
                    //always show persons in these grouping
                        var personNode = new TreeNodeAdv(_commonAgentNameSettings.BuildCommonNameDescription(personSelectorUserDefined))
                        {
                            Tag = new List<Guid> { personSelectorUserDefined.PersonId },
                            LeftImageIndices = new[] { 3 }
                        };
                        
                       
                            currNode.Nodes.Add(personNode);
                            if (_personSelectorView.PreselectedPersonIds.Contains(personSelectorUserDefined.PersonId))
                            {
                                personNode.Checked = true;
								if (_personSelectorView.ExpandSelected)
									currNode.Expanded = true;
                            }

                    if (personSelectorUserDefined.ParentId != null)
                        ((IList<Guid>)currNode.Tag).Add(personSelectorUserDefined.PersonId);
                }
                    
            }
           
            foreach (TreeNodeAdv nodeLevel1 in nodes)
            {
                nodeLevel1.Nodes.Sort();
                foreach (TreeNodeAdv nodeLevel2 in nodeLevel1.Nodes)
                {
                    nodeLevel2.Nodes.Sort();
                    foreach (TreeNodeAdv nodeLevel3 in nodeLevel2.Nodes)
                    {
                        nodeLevel3.Nodes.Sort();
                    }
                }
            }
            _personSelectorView.ResetTreeView(nodes.ToArray());
        }

        public Guid Id
        {
            get { return _value; }
        }

        public string Key
        {
            get { return _value.ToString(); }
        }
    }
}