﻿using System;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Grouping;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Permissions
{
    public partial class PeopleInsertScreen : BaseDialogForm
    {
        private readonly PermissionsExplorer _permissionsExplorerInstance;
        private readonly IComponentContext _container;

        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

        private IPersonSelectorPresenter _personSelectorPresenter;

        protected PeopleInsertScreen(){}

        public PeopleInsertScreen(PermissionsExplorer permissionsExplorer, IComponentContext container)
        {
            _permissionsExplorerInstance = permissionsExplorer;
            _container = container;
            InitializeComponent();
            
            if (!DesignMode)
            {
                SetTexts();
                gradientPanel1.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
                labelTitle.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
            }
        }

        private void buttonAdvInsertClick(object sender, EventArgs e)
        {
            // Load people who are selected by user.
            _permissionsExplorerInstance.SelectedPersonsToAddToRole = _personSelectorPresenter.SelectedPersonGuids;
            if (_personSelectorPresenter.SelectedPersonGuids.Count == 0)
            {
                MessageBox.Show(UserTexts.Resources.SelectAtleastOnePerson, UserTexts.Resources.PeopleInsertScreen, MessageBoxButtons.OK);
                DialogResult = DialogResult.None;
            }
            else
            {
                DialogResult = DialogResult.OK;    
            }
        }

        private void buttonAdvCancelClick(object sender, EventArgs e)
        {
            if (_permissionsExplorerInstance.SelectedPersonsToAddToRole != null)
                _permissionsExplorerInstance.SelectedPersonsToAddToRole.Clear();
            DialogResult = DialogResult.Cancel;
        }

        private void peopleInsertScreenLoad(object sender, EventArgs e)
        {
            _personSelectorPresenter = _container.Resolve<IPersonSelectorPresenter>();
            _personSelectorPresenter.ApplicationFunction = _myApplicationFunction;
            _personSelectorPresenter.ShowPersons = true;
            _personSelectorPresenter.ShowUsers = true;
            var view = (Control)_personSelectorPresenter.View;
            tableLayoutPanel1.Controls.Add(view);
            tableLayoutPanel1.SetCellPosition(view, new TableLayoutPanelCellPosition(0,1) );
            view.Dock = DockStyle.Fill;
           
            var selectorView = _personSelectorPresenter.View;
            selectorView.HideMenu = true;
            _personSelectorPresenter.LoadTabs();
        }

    }
}