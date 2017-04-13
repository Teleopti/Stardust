using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadBuiltInTabsCommand : IExecutableCommand, ILoadGroupPageCommand
    {
    }

    public class LoadBuiltInTabsCommand : ILoadBuiltInTabsCommand
    {
        private readonly PersonSelectorField _loadType;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
        private readonly IPersonSelectorView _personSelectorView;
        private readonly string _rootNodeName;
        private readonly ICommonNameDescriptionSetting _commonAgentNameSettings;
        private readonly IApplicationFunction _applicationFunction;
	    private readonly Guid _optionalColumnId;

	    public LoadBuiltInTabsCommand(
			  PersonSelectorField loadType, 
			  IUnitOfWorkFactory unitOfWorkFactory, 
			  IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository, 
			  IPersonSelectorView personSelectorView, 
			  string rootNodeName, 
			  ICommonNameDescriptionSetting commonAgentNameSettings, 
			  IApplicationFunction applicationFunction, 
			  Guid optionalColumnId)
        {
            _loadType = loadType;
            _unitOfWorkFactory = unitOfWorkFactory;
            _personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
            _personSelectorView = personSelectorView;
            _rootNodeName = rootNodeName;
            _commonAgentNameSettings = commonAgentNameSettings;
            _applicationFunction = applicationFunction;
	        _optionalColumnId = optionalColumnId;
        }

        public void Execute()
        {
            var selectedPeriod = _personSelectorView.SelectedPeriod;
            IList<IPersonSelectorBuiltIn> toNodes;
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
	            toNodes = _personSelectorReadOnlyRepository.GetBuiltIn(selectedPeriod, _loadType, _optionalColumnId);    
            }
            
            // rättigheter
            var auth = PrincipalAuthorization.Current();
            var toRemove = new List<IPersonSelectorBuiltIn>();
			if (_personSelectorView.VisiblePersonIds != null)
			{
				foreach (var toNode in toNodes)
				{
					if (!_personSelectorView.VisiblePersonIds.Contains(toNode.PersonId))
						toRemove.Add(toNode);

				}
			}
			foreach (var personSelectorOrganization in toRemove)
			{
				toNodes.Remove(personSelectorOrganization);
			}
            foreach (var toNode in toNodes)
            {
                if (toNode.PersonId != Guid.Empty)
                {
                    if (!auth.IsPermitted(_applicationFunction.FunctionPath, selectedPeriod.StartDate, toNode))
                        toRemove.Add(toNode);
                }
            }
            foreach (var personSelector in toRemove)
            {
                toNodes.Remove(personSelector);
            }
            //skapa treeviewnoder av det vi fått kvar
            var nodes = new List<TreeNodeAdv>();
            var root = new TreeNodeAdv(_rootNodeName) { LeftImageIndices = new[] { 1 }, Expanded = true, Tag =new List<Guid>() };
            nodes.Add(root);

            var currNode = new TreeNodeAdv("") { LeftImageIndices = new[] { 2 }, Tag = new List<Guid>() };
            
            foreach (var personSelectorBuiltin in toNodes)
            {
                //we could on these have a list of all persons guids underneath on the tag
                if (!string.IsNullOrEmpty(personSelectorBuiltin.Node) && currNode.Text != personSelectorBuiltin.Node)
                {
                    currNode = new TreeNodeAdv(personSelectorBuiltin.Node) { LeftImageIndices = new[] { 2 }, Tag = new List<Guid>() };
                    root.Nodes.Add(currNode);
                }
                    // and here have a list with one guid
                    var personNode = new TreeNodeAdv(_commonAgentNameSettings.BuildCommonNameDescription(personSelectorBuiltin)) { Tag = new List<Guid> { personSelectorBuiltin.PersonId }, LeftImageIndices = new[] { 3 } };
                    // how should we display and how should we sort ?? 
                    // we have a setting for the display of person
                    currNode.Nodes.Add(personNode);
                    if (_personSelectorView.PreselectedPersonIds.Contains(personSelectorBuiltin.PersonId))
                    {
                        personNode.Checked = true;
						if (_personSelectorView.ExpandSelected)
							currNode.Expanded = true;
                    }
                
                ((IList<Guid>)currNode.Tag).Add(personSelectorBuiltin.PersonId);
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

        public string Key
        {
            get { return _loadType.ToString(); }
        }
    }
}