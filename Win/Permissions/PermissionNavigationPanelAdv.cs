using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class PermissionNavigationPanelAdv : BaseUserControlWithUnitOfWork, ISelfDataHandling
    {

        #region Constructor

        public PermissionNavigationPanelAdv()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        #endregion

        #region Event Handling

        #region UserControl

        private void UserControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                businessUnitHierarchyPanel1.ApplicationFunction =
                    ApplicationFunction.FindByPath(DefinedRaptorApplicationFunctionFactory.ApplicationFunctionList,
                                                   DefinedRaptorApplicationFunctionPaths.OpenPermissionPage);
                businessUnitHierarchyPanel1.SetUnitOfWork(UnitOfWork);
                ExchangeData(ExchangeDataOption.ServerToClient);
            }
        }

        #endregion

        #endregion

        #region Interface

        #region ISelfDataHandling Members

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
            throw new NotSupportedException();
        }

        /// <summary>
        /// Persists the data instance.
        /// </summary>
        public void Persist()
        {
            ExchangeData(ExchangeDataOption.ClientToServer);
            UnitOfWork.PersistAll();
            ExchangeData(ExchangeDataOption.ServerToClient);
        }

        /// <summary>
        /// Validates the user edited data in control.
        /// </summary>
        /// <param name="direction">The direction  of dataflow.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
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
                businessUnitHierarchyPanel1.ExchangeData(direction);
            }
        }

        #endregion


        #endregion

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<IPerson> selectedPersons = businessUnitHierarchyPanel1.SelectedPersons;

            if (selectedPersons != null && selectedPersons.Count > 0)
            {
                IPerson person = selectedPersons[0];
                AuthorizationProcessViewScreen screen = new AuthorizationProcessViewScreen(person);
                screen.ShowDialog();
            }
        }

    }
}
