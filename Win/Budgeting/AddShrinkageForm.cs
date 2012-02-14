﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Size = System.Drawing.Size;

namespace Teleopti.Ccc.Win.Budgeting
{
    public partial class AddShrinkageForm : BaseRibbonForm, IAddShrinkageForm
    {
        internal event EventHandler<CustomEventArgs<CustomShrinkageUpdatedEventArgs>> CustomShrinkageAdded;
        private const int maxLength = 25;
        private string _helpId;
        private readonly AddShrinkagePresenter _presenter;
        private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();

        public AddShrinkageForm(IBudgetGroup budgetGroup, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            InitializeComponent();
            _helpId = Name;
            if (!DesignMode) SetTexts();
            _presenter = new AddShrinkagePresenter(this, new AddShrinkageModel(budgetGroup, unitOfWorkFactory, repositoryFactory));
            _presenter.Initialize();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            textBoxShrinkageName.MaxLength = maxLength;
            if (checkBoxInclude != null)
            {
                hideAbsencePanel();
                initializeAbsences();
            }
        }

        private void initializeAbsences()
        {
            listBoxAbsences.DisplayMember = "Name";
            listBoxAddedAbsences.DisplayMember = "Name";
            listBoxAbsences.Items.AddRange(_presenter.Absences.ToArray());
        }

        public void HideIncludedInRequestAllowance()
        {
            SuspendLayout();
            var height = tableLayoutPanelFields.GetRowHeights()[1];
            tableLayoutPanelFields.RowStyles[1].Height = 0;
            tableLayoutPanelFields.Controls.Remove(tableLayoutPanelAllowance);
            Size = new Size(Size.Width, Size.Height - height);
            tableLayoutPanelAllowance = null;
            checkBoxInclude = null;
            ResumeLayout();
        }

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			textBoxShrinkageName.Focus();
			textBoxShrinkageName.SelectAll();
		}

        private void buttonAdvSave_Click(object sender, EventArgs e)
        {
            var shrinkageName = textBoxShrinkageName.Text.Trim();
            if(string.IsNullOrEmpty(shrinkageName))
            {
                errorProvider.SetError(textBoxShrinkageName, UserTexts.Resources.TheInputTextCanNotBeEmptyDot);
                errorProvider.SetIconPadding(textBoxShrinkageName,-20);
                return;
            }
        	var handler = CustomShrinkageAdded;
            if (handler!= null)
            {
                _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(
                    () =>
                        {
                            saveAddedCustomShrinkage(shrinkageName);
                            notifySubsribers(handler);
                        });
            }
            Close();
        }

        private void notifySubsribers(EventHandler<CustomEventArgs<CustomShrinkageUpdatedEventArgs>> handler)
        {
            var eventArgs = new CustomShrinkageUpdatedEventArgs {CustomShrinkage = _presenter.CustomShrinkageAdded};
            handler.Invoke(this, new CustomEventArgs<CustomShrinkageUpdatedEventArgs>(eventArgs));
        }

        private void saveAddedCustomShrinkage(string shrinkageName)
        {
            var customShrinkage = new CustomShrinkage(shrinkageName);
            if (checkBoxInclude != null)
                customShrinkage.IncludedInAllowance = checkBoxInclude.Checked;
            foreach (var absence in listBoxAddedAbsences.Items)
            {
                customShrinkage.AddAbsence((IAbsence) absence);
            }
            _presenter.Save(customShrinkage);
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public override string HelpId
        {
            get
            {
                return _helpId;
            }
        }

        public void SetHelpId(string helpId)
        {
            _helpId = helpId;
        }

        private void buttonAdvAddAll_Click(object sender, EventArgs e)
        {
            listBoxAddedAbsences.Items.AddRange(listBoxAbsences.Items);
            listBoxAbsences.Items.Clear();
        }

        private void buttonAdvAddSelected_Click(object sender, EventArgs e)
        {
            _presenter.AddAbsences();
        }

        public void AddSelectedAbsences()
        {
            foreach (var item in listBoxAbsences.SelectedItems)
            {
                listBoxAddedAbsences.Items.Add(item);
            }

            foreach (var item in listBoxAddedAbsences.Items)
            {
                listBoxAbsences.Items.Remove(item);
            }
        }
        
        public void RemoveSelectedAbsences()
        {
            foreach (var selectedItem in listBoxAddedAbsences.SelectedItems)
            {
                listBoxAbsences.Items.Add(selectedItem);
            }

            foreach (var item in listBoxAbsences.Items)
            {
                listBoxAddedAbsences.Items.Remove(item);
            }
        }

        private void buttonAdvRemoveSelected_Click(object sender, EventArgs e)
        {
            _presenter.RemoveAllAbsences();
        }

        private void buttonAdvRemoveAll_Click(object sender, EventArgs e)
        {
            listBoxAbsences.Items.AddRange(listBoxAddedAbsences.Items);
            listBoxAddedAbsences.Items.Clear();
        }

        private void listBoxAbsences_DoubleClick(object sender, EventArgs e)
        {
            _presenter.AddAbsences();
        }

        private void listBoxAddedAbsences_DoubleClick(object sender, EventArgs e)
        {
            _presenter.RemoveAllAbsences();
        }

        private void checkBoxInclude_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxInclude.Checked)
                showAbsencePanel();
            else
                hideAbsencePanel();
        }

        private void hideAbsencePanel()
        {
            SuspendLayout();
            var height = tableLayoutPanelAbsence.Height;
            tableLayoutPanelAllowance.Controls.Remove(tableLayoutPanelAbsence);
            tableLayoutPanelAllowance.RowStyles[1].Height = 0;
            tableLayoutPanelFields.RowStyles[1].Height = tableLayoutPanelAllowance.RowStyles[0].Height;
            Size = new Size(Size.Width, Size.Height - height);
            ResumeLayout();
        }

        private void showAbsencePanel()
        {
            SuspendLayout();
            var absencePanelHeight = tableLayoutPanelAbsence.Height;
            tableLayoutPanelAllowance.RowStyles[1].Height = absencePanelHeight;
            tableLayoutPanelAllowance.Controls.Add(tableLayoutPanelAbsence, 0, 1);
            tableLayoutPanelFields.RowStyles[1].Height = tableLayoutPanelAllowance.RowStyles[0].Height +
                                                         absencePanelHeight;
            Size = new Size(Size.Width, Size.Height + absencePanelHeight);
            ResumeLayout();
        }

        private void textBoxExt1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxShrinkageName.Text.Trim()) &&
                !string.IsNullOrEmpty(errorProvider.GetError(textBoxShrinkageName)))
                errorProvider.SetError(textBoxShrinkageName, string.Empty);
        }
    }
}
