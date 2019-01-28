using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands
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
			var auth = Domain.Security.Principal.PrincipalAuthorization.Current_DONTUSE();
			var toRemove = new List<IPersonSelectorBuiltIn>();
			if (_personSelectorView.VisiblePersonIds != null)
			{
				toRemove.AddRange(toNodes.Where(toNode => !_personSelectorView.VisiblePersonIds.Contains(toNode.PersonId)));
			}
			foreach (var personSelectorOrganization in toRemove)
			{
				toNodes.Remove(personSelectorOrganization);
			}
			foreach (var toNode in toNodes)
			{
				if (toNode.PersonId == Guid.Empty) continue;

				if (!auth.IsPermitted(_applicationFunction.FunctionPath, selectedPeriod.StartDate, toNode))
					toRemove.Add(toNode);
			}
			foreach (var personSelector in toRemove)
			{
				toNodes.Remove(personSelector);
			}
			//skapa treeviewnoder av det vi fått kvar
			var nodes = new List<TreeNodeAdv>();
			var root = new TreeNodeAdv(_rootNodeName) {LeftImageIndices = new[] {1}, Expanded = true, Tag = new List<Guid>()};
			nodes.Add(root);

			var toNodesByNode = toNodes.GroupBy(t => t.Node.Trim());
			foreach (var toNode in toNodesByNode)
			{
				var personNodes = toNode.Select(personSelectorBuiltin => new {
					PersonId = personSelectorBuiltin.PersonId,
					Node =
					new TreeNodeAdv(_commonAgentNameSettings.BuildFor(personSelectorBuiltin))
					{
						Tag = new List<Guid> { personSelectorBuiltin.PersonId },
						LeftImageIndices = new[] { 3 }
					}
				}).ToArray();

				var currNode = new TreeNodeAdv(toNode.Key) {LeftImageIndices = new[] {2}, Tag = personNodes.Select(n => n.PersonId).ToList()};
				root.Nodes.Add(currNode);

				// and here have a list with one guid
				
				// how should we display and how should we sort ?? 
				// we have a setting for the display of person
				currNode.Nodes.AddRange(personNodes.Select(n => n.Node).ToArray());

				var preselectPersonsInNode = personNodes.Where(n => _personSelectorView.PreselectedPersonIds.Contains(n.PersonId));
				foreach (var personNode in preselectPersonsInNode)
				{
					personNode.Node.Checked = true;
					if (_personSelectorView.ExpandSelected)
						currNode.Expanded = true;
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

        public string Key => _loadType.ToString();
	}
}