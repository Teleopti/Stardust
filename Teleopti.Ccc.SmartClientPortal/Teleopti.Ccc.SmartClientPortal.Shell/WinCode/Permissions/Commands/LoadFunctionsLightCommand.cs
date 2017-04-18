using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands
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
                var checkBoxnodes = new List<TreeNodeAdv>();
                var allNodes = new List<TreeNodeAdv>();
                
                foreach (IFunctionLight function in rootApplicationFunctions)
                {
                    string  name = LanguageResourceHelper.Translate(function.ResourceName);
                    if (string.IsNullOrEmpty(name))
                        name = function.Name;
                    var rootNode = new TreeNodeAdv(name)
                    {
                        TagObject = function.Id,
                        CheckState = CheckState.Unchecked,
                        Expanded = true
                    };

                    var cbNode = new TreeNodeAdv(name)
                    {
                        TagObject = function.Id,
                        CheckState = CheckState.Unchecked,
                        ShowCheckBox = true,
                        Expanded = true
                    };

                    //disableFunctionsNotLicensed(function, rootNode);

                    recursivelyAddChildNodes(rootNode, functions, null);
                    nodes.Add(rootNode);

                    allNodes.Add(cbNode);
                    recursivelyAddChildNodes(cbNode, functions, allNodes);
                    checkBoxnodes.Add(cbNode);
                }
                // smacka in trädet i vyn
                _permissionViewerRoles.FillFunctionTree(checkBoxnodes.ToArray(), nodes.ToArray(), allNodes.ToArray());
            }
        }

        private static void recursivelyAddChildNodes(TreeNodeAdv treeNode, IEnumerable<IFunctionLight> functions,  IList<TreeNodeAdv> addNodeToThis)
        {
            var parentApplicationFunction = (Guid)treeNode.TagObject;
            if (parentApplicationFunction.Equals(Guid.Empty)) return;

            foreach (IFunctionLight function in
                    functions.Where(f => parentApplicationFunction.Equals(f.Parent)))
            {
                string name = LanguageResourceHelper.Translate(function.ResourceName);
                if (string.IsNullOrEmpty(name))
                    name = function.Name;
                var rootNode = new TreeNodeAdv(name)
                {
                    TagObject = function.Id,
                    CheckState = CheckState.Unchecked,
                    ShowCheckBox =  treeNode.ShowCheckBox,
                    Expanded = true
                };

                //disableFunctionsNotLicensed(function, rootNode);

                treeNode.Nodes.Add(rootNode);
                if (addNodeToThis != null)
                    addNodeToThis.Add(rootNode);
                recursivelyAddChildNodes(rootNode, functions, addNodeToThis);
            }
        }
    }
}