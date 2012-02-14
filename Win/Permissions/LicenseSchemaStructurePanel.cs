using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlBinders;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class LicenseSchemaStructurePanel : BaseUserControlWithUnitOfWork, ISelfDataHandling
    {
        private ITreeItem<TreeNode> _rootItem;

        public LicenseSchemaStructurePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Exchances the data between the controls and the datasource object.
        /// </summary>
        /// <param name="direction">The direction of dataflow.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.ServerToClient)
            {
                LoadLicenseSchemaStructureTree();
            }
        }

        /// <summary>
        /// Sets the unit of work.
        /// Typically calls for the ChangeObjectsUnitOfWork method in base class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public void SetUnitOfWork(IUnitOfWork value)
        {
            ChangeObjectsUnitOfWork(this, value);
        }

        /// <summary>
        /// Persists the data instance.
        /// </summary>
        public void Persist()
        {}

        /// <summary>
        /// Fetches all the roles.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        private static LicenseSchema FetchLicenceSchema()
        {
            return LicenseSchema.ActiveLicenseSchema;
        }

        /// <summary>
        /// Fetches all the roles.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        private IList<IApplicationFunction> FetchAllApplicationFunctions()
        {
            ApplicationFunctionRepository rep = new ApplicationFunctionRepository(UnitOfWork);
            IList<IApplicationFunction> applicationFunctionList = rep.GetAllApplicationFunctionSortedByCode();
            return applicationFunctionList;
        }

        #endregion

        /// <summary>
        /// Loads (gets and fills data) the function structure tree.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/6/2007
        /// </remarks>
        private void LoadLicenseSchemaStructureTree()
        {
            IList<IApplicationFunction> allApplicationFunctions = FetchAllApplicationFunctions();
            LicenseSchema licenceSchema = FetchLicenceSchema();
            _rootItem = new TreeItem<TreeNode>("Raptor licence schema");
            _rootItem.ImageIndex = 0;

            ITreeItem<TreeNode> raptorSchemaItem = new TreeItem<TreeNode>(_rootItem, DefinedLicenseSchemaCodes.TeleoptiCccSchema);
            raptorSchemaItem.ImageIndex = 1;

            foreach (LicenseOption licenceOption in licenceSchema.LicenseOptions)
            {
                ITreeItem<TreeNode> parentSchemaItem = _rootItem.FindItem(licenceOption.LicenseSchemaCode, RangeOption.Children);

                ITreeItem<TreeNode> licenseItem = TreeItemEntityHelper.CreateTreeItem(licenceOption);
                licenseItem.Parent = parentSchemaItem;
                licenseItem.ImageIndex = 2;

                // Functions
                licenceOption.EnableApplicationFunctions(allApplicationFunctions);
                foreach (ApplicationFunction function in licenceOption.EnabledApplicationFunctions)
                {
                    AuthorizationEntityListBoxPresenter functionPresenter = new AuthorizationEntityListBoxPresenter(function);
                    ITreeItem<TreeNode> functionItem = TreeItemEntityHelper.CreateTreeItem(functionPresenter);
                    functionItem.Parent = licenseItem;
                    functionItem.ImageIndex = 3;
                }
            }

            TreeViewBinder visualizer = new TreeViewBinder(treeViewLicenseSchema, _rootItem);
            visualizer.Display(2);

        }

        #endregion
    }
}
