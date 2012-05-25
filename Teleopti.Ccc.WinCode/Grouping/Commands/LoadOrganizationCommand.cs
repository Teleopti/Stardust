using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
{
    public interface ILoadGroupPageCommand
    {
        string Key { get; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadOrganizationCommand : IExecutableCommand, ILoadGroupPageCommand
    {
    }

    public class LoadOrganizationCommand : ILoadOrganizationCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPersonSelectorView _view;
        private readonly ICommonNameDescriptionSetting _commonAgentNameSettings;
        private readonly IApplicationFunction _applicationFunction;
        private readonly bool _showPersons;
        private readonly bool _loadUsers;

        public LoadOrganizationCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, 
            IPersonSelectorView view, ICommonNameDescriptionSetting commonAgentNameSettings, IApplicationFunction applicationFunction, bool showPersons, bool loadUsers)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _view = view;
            _commonAgentNameSettings = commonAgentNameSettings;
            _applicationFunction = applicationFunction;
            _showPersons = showPersons;
            _loadUsers = loadUsers;
        }

        public void Execute()
        {
            var loadUser = TeleoptiPrincipal.Current.PrincipalAuthorization.EvaluateSpecification(new AllowedToSeeUsersNotInOrganizationSpecification(_applicationFunction.FunctionPath));
            if (!_loadUsers)
                loadUser = false;
            var dateOnlyPeriod = _view.SelectedPeriod;
            IList<IPersonSelectorOrganization> toNodes;
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                IPersonSelectorReadOnlyRepository rep = _repositoryFactory.CreatePersonSelectorReadOnlyRepository(uow);
                toNodes = rep.GetOrganization(dateOnlyPeriod, loadUser);    
            }
            
            // rättigheter
            var auth = TeleoptiPrincipal.Current.PrincipalAuthorization;
            var toRemove = new List<IPersonSelectorOrganization>();
            foreach (var toNode in toNodes)
            {
                if (toNode.PersonId != new Guid())
                {
                    if (!auth.IsPermitted(_applicationFunction.FunctionPath, dateOnlyPeriod.StartDate, toNode))
                        toRemove.Add(toNode);
                }
            }
            foreach (var personSelectorOrganization in toRemove)
            {
                toNodes.Remove(personSelectorOrganization);
            }
            //skapa treeviewnoder av det vi fått kvar
            var nodes = new List<TreeNodeAdv>();
            var root = new TreeNodeAdv(((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Name) { LeftImageIndices = new[] { 0 }, Expanded = true, Tag = new List<Guid>(), TagObject = new List<Guid>() };
            nodes.Add(root);
            var usersNode = new TreeNodeAdv(Resources.UserName) { LeftImageIndices = new[] { 1 } ,Tag = new List<Guid>()};
            root.Nodes.Add(usersNode);
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
                    personNode = new TreeNodeAdv(_commonAgentNameSettings.BuildCommonNameDescription(personSelectorOrganization)) 
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
                        if(personNode.Checked)
                        {
                            currTeam.Expanded = true;
                            currSite.Expanded = true;
                        }
                    }
                    ((IList<Guid>)currTeam.Tag).Add(personSelectorOrganization.PersonId);
                }
            }
            if (usersNode.Nodes.Count == 0)
                root.Nodes.Remove(usersNode);

            
            _view.ResetTreeView(nodes.ToArray());
        }

        public string Key
        {
            get { return "Organization"; }
        }
    }
}