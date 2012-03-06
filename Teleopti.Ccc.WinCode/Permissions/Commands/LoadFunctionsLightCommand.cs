using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadFunctionsLightCommand : IExecutableCommand { }

    public class LoadFunctionsLightCommand : ILoadFunctionsLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadFunctionsLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var functions = _repositoryFactory.CreateApplicationRolePersonRepository(uow).Functions();
                IEnumerable<IFunctionLight> rootApplicationFunctions = functions.Where(af => af.Parent.Equals(Guid.Empty));
                var nodes = new List<TreeNodeAdv>();
                foreach (IFunctionLight function in rootApplicationFunctions)
                {
                    string name = "";
                    if (!string.IsNullOrEmpty(function.ResourceName))
                        name = LanguageResourceHelper.Translate(function.ResourceName);
                    var rootNode = new TreeNodeAdv(name)
                    {
                        TagObject = function.Id,
                        Tag = 1,
                        CheckState = CheckState.Unchecked
                    };

                    //disableFunctionsNotLicensed(function, rootNode);

                    recursivelyAddChildNodes(rootNode, functions);
                    nodes.Add(rootNode);
                }
                // smacka in trädet i vyn
                _permissionViewerRoles.FillFunctionTree(null, nodes.ToArray());
            }
        }

        private static void recursivelyAddChildNodes(TreeNodeAdv treeNode, IEnumerable<IFunctionLight> functions)
        {
            if (functions == null) return;
            var parentApplicationFunction = (Guid)treeNode.TagObject;
            if (parentApplicationFunction.Equals(Guid.Empty)) return;

            foreach (IFunctionLight function in
                    functions.Where(f => parentApplicationFunction.Equals(f.Parent)))
            {
                string name = "";
                if (!string.IsNullOrEmpty(function.ResourceName))
                    name = LanguageResourceHelper.Translate(function.ResourceName);
                var rootNode = new TreeNodeAdv(name)
                {
                    TagObject = function.Id,
                    Tag = 1,
                    CheckState = CheckState.Unchecked
                };

                //disableFunctionsNotLicensed(function, rootNode);

                treeNode.Nodes.Add(rootNode);
                recursivelyAddChildNodes(rootNode, functions);
            }
        }
    }
}