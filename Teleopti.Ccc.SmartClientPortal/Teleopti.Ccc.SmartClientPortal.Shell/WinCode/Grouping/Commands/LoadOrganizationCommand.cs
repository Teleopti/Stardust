using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands
{
    public interface ILoadGroupPageCommand
    {
        string Key { get; }
    }

    public interface ILoadOrganizationCommand : IExecutableCommand, ILoadGroupPageCommand
    {
    }

    public class LoadOrganizationCommand : ILoadOrganizationCommand, IDisposable
    {
        private  IUnitOfWorkFactory _unitOfWorkFactory;
        private  IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
        private  IPersonSelectorView _view;
        private  ICommonNameDescriptionSetting _commonAgentNameSettings;
        private  IApplicationFunction _applicationFunction;
        private readonly bool _showPersons;
        private readonly bool _loadUsers;

        public LoadOrganizationCommand(IUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository, 
            IPersonSelectorView view, ICommonNameDescriptionSetting commonAgentNameSettings, IApplicationFunction applicationFunction, bool showPersons, bool loadUsers)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
            _view = view;
            _commonAgentNameSettings = commonAgentNameSettings;
            _applicationFunction = applicationFunction;
            _showPersons = showPersons;
            _loadUsers = loadUsers;
        }

		public void Execute()
        {
            var loadUser = Domain.Security.Principal.PrincipalAuthorization.Current().EvaluateSpecification(new AllowedToSeeUsersNotInOrganizationSpecification(_applicationFunction.FunctionPath));
            if (!_loadUsers)
                loadUser = false;
            var dateOnlyPeriod = _view.SelectedPeriod;
            IList<IPersonSelectorOrganization> toNodes;
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                toNodes = _personSelectorReadOnlyRepository.GetOrganization(dateOnlyPeriod, loadUser);    
            }
            
            toNodes = removeDuplicates(toNodes);
			// Permissions
			var auth = Domain.Security.Principal.PrincipalAuthorization.Current();

            var toRemove = new List<IPersonSelectorOrganization>();
			if(_view.VisiblePersonIds != null)
			{
				toRemove.AddRange(toNodes.Where(n => !_view.VisiblePersonIds.Contains(n.PersonId)));
			}

			foreach (var personSelectorOrganization in toRemove)
			{
				toNodes.Remove(personSelectorOrganization);
			}
            foreach (var toNode in toNodes)
			{
				if (toNode.PersonId == Guid.Empty) continue;

				if (!auth.IsPermitted(_applicationFunction.FunctionPath, dateOnlyPeriod.StartDate, toNode))
					toRemove.Add(toNode);
			}
            foreach (var personSelectorOrganization in toRemove)
            {
                toNodes.Remove(personSelectorOrganization);
            }
            //Create treeviewnoder of what we have left
            var nodes = new List<TreeNodeAdv>();
            var root = new TreeNodeAdv(((ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnitName) { LeftImageIndices = new[] { 0 }, Expanded = true, Tag = new List<Guid>(), TagObject = new List<Guid>() };
            nodes.Add(root);
            var usersNode = new TreeNodeAdv(Resources.UserName) { LeftImageIndices = new[] { 1 } ,Tag = new List<Guid>()};
            var currSite = new TreeNodeAdv("");
        	var currTeamText = ""; 
            var currTeam = new TreeNodeAdv("");
            
            foreach (var personSelectorOrganization in toNodes)
            {
                //we could on these have a list of all persons guids underneath on the tag
                if (!string.IsNullOrEmpty(personSelectorOrganization.Site) && currSite.Text != personSelectorOrganization.Site)
                {
                    currSite = new TreeNodeAdv(personSelectorOrganization.Site) { LeftImageIndices = new[] { 1 }, Tag = new List<Guid>(), TagObject = new List<Guid>() };
                    root.Nodes.Add(currSite);
                	currTeamText = "";
                }
				if (!string.IsNullOrEmpty(personSelectorOrganization.Team) && currTeamText != personSelectorOrganization.Team && personSelectorOrganization.TeamId.HasValue)
				{
					currTeamText = personSelectorOrganization.Team;
					currTeam = new TreeNodeAdv(currTeamText) { LeftImageIndices = new[] { 2 }, Tag = new List<Guid>(), TagObject = new List<Guid> { personSelectorOrganization.TeamId.Value } };
                    currSite.Nodes.Add(currTeam);
                    ((IList<Guid>)currSite.TagObject).Add(personSelectorOrganization.TeamId.Value);
                    ((IList<Guid>)root.TagObject).Add(personSelectorOrganization.TeamId.Value);
                }
                TreeNodeAdv personNode = null;
                // and here have a list with one guid
                if(_showPersons)
                {
                    personNode = new TreeNodeAdv(_commonAgentNameSettings.BuildFor(personSelectorOrganization)) 
                    { Tag = new List<Guid> { personSelectorOrganization.PersonId }, LeftImageIndices = new[] { 3 } };
                    if (_view.PreselectedPersonIds.Contains(personSelectorOrganization.PersonId))
                    {
                        personNode.Checked = true;
                    }
                }
                // how should we display and how should we sort ?? 
                if (string.IsNullOrEmpty(personSelectorOrganization.Team))
                {
                    if (_showPersons)
                    {
                        usersNode.Nodes.Add(personNode);
                        ((IList<Guid>)usersNode.Tag).Add(personSelectorOrganization.PersonId);
                    }
                }
                else
                {
                    if(_showPersons)
                    {
                        currTeam.Nodes.Add(personNode);
						if (_view.ExpandSelected && personNode.Checked)
                        {
                            currTeam.Expanded = true;
                            currSite.Expanded = true;
                        }
                    }
                    ((IList<Guid>)currTeam.Tag).Add(personSelectorOrganization.PersonId);
                }
            }
            
            foreach (TreeNodeAdv nodeLevel1 in nodes)
            {
                nodeLevel1.Nodes.Sort();
                if (usersNode.Nodes.Count > 0)
                    nodeLevel1.Nodes.Insert(0, usersNode);
                foreach (TreeNodeAdv nodeLevel2 in nodeLevel1.Nodes)
                {
                    nodeLevel2.Nodes.Sort();
                    foreach (TreeNodeAdv nodeLevel3 in nodeLevel2.Nodes)
                    {
                        nodeLevel3.Nodes.Sort();
                    }
                }
            }
            _view.ResetTreeView(nodes.ToArray());

        }

    	private static IList<IPersonSelectorOrganization> removeDuplicates(IEnumerable<IPersonSelectorOrganization> toNodes)
    	{
    		var result = from t in toNodes
    		             group t by
    		             	t.PersonId.ToString() + t.TeamId.GetValueOrDefault().ToString() +
    		             	t.SiteId.GetValueOrDefault().ToString()
    		             into g
    		             select g.First();
    		return result.ToList();
    	}

    	public string Key => "Organization";

		public void Dispose()
	    {
		    _view = null;
			_unitOfWorkFactory = null;
			_personSelectorReadOnlyRepository = null;
			_commonAgentNameSettings = null;
			_applicationFunction = null;
		}
    }

}