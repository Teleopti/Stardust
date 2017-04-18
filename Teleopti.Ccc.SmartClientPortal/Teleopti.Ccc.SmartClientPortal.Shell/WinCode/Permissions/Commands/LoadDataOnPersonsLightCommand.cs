using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadDataOnPersonsLightCommand : IExecutableCommand { }

    public class LoadDataOnPersonsLightCommand : ILoadDataOnPersonsLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadDataOnPersonsLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var nodes = _permissionViewerRoles.AllDataNodes;
                foreach (var treeNodeAdv in nodes)
                {
                    treeNodeAdv.Checked = false;
                }
                var rep = _repositoryFactory.CreateApplicationRolePersonRepository(uow);
                var per = _permissionViewerRoles.SelectedPerson;
                var data = rep.AvailableData(per);
                var dataRangeOptions = rep.DataRangeOptions(per);
                foreach (var treeNodeAdv in nodes)
                {
                    var id = treeNodeAdv.TagObject;
                    if (id.GetType().Equals(typeof(Guid)))
                    {
                        if (data.Contains((Guid)id))
                            treeNodeAdv.Checked = true;
                    }
                    if (id.GetType().Equals(typeof(int)))
                    {
                        if (dataRangeOptions.Contains((int)id))
                            treeNodeAdv.Checked = true;
                    }
                }
                
            }
        }
    }
}