using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions
{
    public interface IPermissionViewerRolesPresenter
    {
        void ShowViewer();
        bool Unloaded { get; }
        void CloseViewer();
    }

    public class PermissionViewerRolesPresenter : IPermissionViewerRolesPresenter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPermissionViewerRoles _permissionViewerRoles;
        private readonly ILoadPersonsLightCommand _loadPersonsLightCommand;
        private readonly ILoadRolesOnPersonLightCommand _loadRolesOnPersonLightCommand;
        private readonly ILoadFunctionsOnPersonLightCommand _loadFunctionsOnPersonLightCommand;
        private readonly ILoadFunctionsLightCommand _loadFunctionsLightCommand;
        private readonly ILoadPersonsWithFunctionLightCommand _loadPersonsWithFunctionLightCommand;
        private readonly ILoadRolesWithFunctionLightCommand _loadRolesWithFunctionLightCommand;
        private readonly ILoadDataOnPersonsLightCommand _loadDataOnPersonsLightCommand;
        private readonly ILoadRolesAndPersonsOnDataLightCommand _loadRolesAndPersonsOnDataLightCommand;
        private readonly ILoadRolesAndPersonsOnDataRangeLightCommand _loadRolesAndPersonsOnDataRangeLightCommand;
        private bool _initialLoadDone;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PermissionViewerRolesPresenter(IEventAggregator eventAggregator, IPermissionViewerRoles permissionViewerRoles, 
            ILoadPersonsLightCommand loadPersonsLightCommand, ILoadRolesOnPersonLightCommand loadRolesOnPersonLightCommand, ILoadFunctionsOnPersonLightCommand loadFunctionsOnPersonLightCommand,
            ILoadFunctionsLightCommand loadFunctionsLightCommand, ILoadPersonsWithFunctionLightCommand loadPersonsWithFunctionLightCommand,
            ILoadRolesWithFunctionLightCommand loadRolesWithFunctionLightCommand, ILoadDataOnPersonsLightCommand loadDataOnPersonsLightCommand, 
            ILoadRolesAndPersonsOnDataLightCommand loadRolesAndPersonsOnDataLightCommand, ILoadRolesAndPersonsOnDataRangeLightCommand loadRolesAndPersonsOnDataRangeLightCommand)
        {
            _eventAggregator = eventAggregator;
            _initialLoadDone = false;
            _permissionViewerRoles = permissionViewerRoles;
            _loadPersonsLightCommand = loadPersonsLightCommand;
            _loadRolesOnPersonLightCommand = loadRolesOnPersonLightCommand;
            _loadFunctionsOnPersonLightCommand = loadFunctionsOnPersonLightCommand;
            _loadFunctionsLightCommand = loadFunctionsLightCommand;
            _loadPersonsWithFunctionLightCommand = loadPersonsWithFunctionLightCommand;
            _loadRolesWithFunctionLightCommand = loadRolesWithFunctionLightCommand;
            _loadDataOnPersonsLightCommand = loadDataOnPersonsLightCommand;
            _loadRolesAndPersonsOnDataLightCommand = loadRolesAndPersonsOnDataLightCommand;
            _loadRolesAndPersonsOnDataRangeLightCommand = loadRolesAndPersonsOnDataRangeLightCommand;

            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Subscribe(loadRolePersonsAndFunctions);
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Subscribe(loadFunctionPersonsAndRoles);
            _eventAggregator.GetEvent<DataPersonsAndRolesNeedLoad>().Subscribe(loadPersonsAndRolesOnData);
            _eventAggregator.GetEvent<DataRangePersonsAndRolesNeedLoad>().Subscribe(loadPersonsAndRolesOnDataRange);

            _eventAggregator.GetEvent<PermissionsViewerUnloaded>().Subscribe(permissionsViewerUnloaded);
        }

        private void loadPersonsAndRolesOnDataRange(string obj)
        {
            try
            {
                _loadRolesAndPersonsOnDataRangeLightCommand.Execute();
            }
            catch (Infrastructure.Foundation.DataSourceException e)
            {
                _permissionViewerRoles.ShowDataSourceException(e);
            }
        }

        private void loadPersonsAndRolesOnData(string obj)
        {
            try
            {
                _loadRolesAndPersonsOnDataLightCommand.Execute();
            }
            catch (Infrastructure.Foundation.DataSourceException e)
            {
                _permissionViewerRoles.ShowDataSourceException(e);
            }
        }

        private void permissionsViewerUnloaded(string obj)
        {
            Unloaded = true;
        }

        private void loadFunctionPersonsAndRoles(string obj)
        {
            if (_permissionViewerRoles.SelectedFunction.Equals(Guid.Empty)) return;
            try
            {
                _loadPersonsWithFunctionLightCommand.Execute();
                _loadRolesWithFunctionLightCommand.Execute();
            }
            catch (Infrastructure.Foundation.DataSourceException e)
            {
                _permissionViewerRoles.ShowDataSourceException(e);
            } 
        }

        private void loadRolePersonsAndFunctions(string obj)
        {
            if(_permissionViewerRoles.SelectedPerson.Equals(Guid.Empty)) return;
            try
            {
                _loadRolesOnPersonLightCommand.Execute();
                _loadFunctionsOnPersonLightCommand.Execute();
                _loadDataOnPersonsLightCommand.Execute();
            }
            catch (Infrastructure.Foundation.DataSourceException e)
            {
                _permissionViewerRoles.ShowDataSourceException(e);
            } 
        }

        private void initializeDataTreeRangeOptions()
        {
            var treeNodes = new List<TreeNodeAdv>();
            var dataNodes = new List<TreeNodeAdv>();
            var allTreeNodes = new List<TreeNodeAdv>();

            foreach (AvailableDataRangeOption t in Enum.GetValues(typeof(AvailableDataRangeOption)))
            {
                if (t == AvailableDataRangeOption.None) continue; // skip the None check box

                var node = new TreeNodeAdv
                {
                    ShowCheckBox = true,
                    Text = Enum.GetName(typeof(AvailableDataRangeOption), t),
                    TagObject = (int)t,
                    Tag = t
                };

                treeNodes.Add(node);
                allTreeNodes.Add(node);

                var dataNode = new TreeNodeAdv
                {
                    ShowCheckBox = false,
                    Text = Enum.GetName(typeof(AvailableDataRangeOption), t),
                    TagObject = (int)t,
                    Tag = t
                };
                dataNodes.Add(dataNode);
            }
        
            IBusinessUnit bu = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit;

            var buNode = new TreeNodeAdv
            {
                ShowCheckBox = true,
                Text = bu.Description.Name,
                TagObject = bu.Id,
                Tag = 1
            };

            treeNodes.Add(buNode);
            allTreeNodes.Add(buNode);

            var dataBuNode = new TreeNodeAdv
            {
                ShowCheckBox = false,
                Text = bu.Description.Name,
                TagObject = bu.Id,
                Tag = 1
            };
            dataNodes.Add(dataBuNode);

            foreach (ISite site in bu.SiteCollection)
            {
                if (((IDeleteTag)site).IsDeleted) continue;
                var siteNode = new TreeNodeAdv
                {
                    ShowCheckBox = true,
                    Text = site.Description.Name,
                    TagObject = site.Id,
                    Tag = 2
                };
                buNode.Nodes.Add(siteNode);
                allTreeNodes.Add(siteNode);

                var dataSiteNode = new TreeNodeAdv
                {
                    ShowCheckBox = false,
                    Text = site.Description.Name,
                    TagObject = site.Id,
                    Tag = 2
                };
                dataBuNode.Nodes.Add(dataSiteNode);

                foreach (ITeam team in site.TeamCollection)
                {
                    if (((IDeleteTag)team).IsDeleted) continue;
                    var teamNode = new TreeNodeAdv
                    {
                        ShowCheckBox = true,
                        Text = team.Description.Name,
                        TagObject = team.Id,
                        Tag = 3
                    };
                    siteNode.Nodes.Add(teamNode);
                    allTreeNodes.Add(teamNode);

                    var dataTeamNode = new TreeNodeAdv
                    {
                        ShowCheckBox = false,
                        Text = team.Description.Name,
                        TagObject = team.Id,
                        Tag = 3
                    };
                    dataSiteNode.Nodes.Add(dataTeamNode);
                }
            }
            _permissionViewerRoles.FillDataTree(treeNodes.ToArray(),dataNodes.ToArray(), allTreeNodes.ToArray());

        }

        

        public void ShowViewer()
        {
            if (!_initialLoadDone)
            {
                try
                {
                    _loadPersonsLightCommand.Execute();
                    _loadFunctionsLightCommand.Execute();
                    initializeDataTreeRangeOptions();
                    _initialLoadDone = true;
                }
                catch (Infrastructure.Foundation.DataSourceException e)
                {
                    _permissionViewerRoles.ShowDataSourceException(e);
                    return;
                }
            }
            
            _permissionViewerRoles.Show();
            _permissionViewerRoles.BringToFront();
        }

        public bool Unloaded { get; private set; }

        public void CloseViewer()
        {
            _permissionViewerRoles.Close();
        }
    }
}