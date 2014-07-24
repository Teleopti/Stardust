using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class OptionalColumnsControl : BaseUserControl, ISettingPage
    {
        private List<IOptionalColumn> _optionalColumnList;
        private const string PersonTableName = "Person";
        private const short ItemDiffernce = 1;
        private const short InvalidItemIndex = -1;
        private const short FirstItemIndex = 0;
        public OptionalColumnRepository Repository { get; private set; }
        public IUnitOfWork UnitOfWork { get; private set; }
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public int LastItemIndex
        {
            get { return comboBoxOptionalColumns.Items.Count - ItemDiffernce; }
        }

        public IOptionalColumn SelectedOptionalColumn
        {
            get { return comboBoxOptionalColumns.SelectedItem as IOptionalColumn; }
        }

        public OptionalColumnsControl()
        {
			InitializeComponent();

			// Bind events.
			comboBoxOptionalColumns.SelectedIndexChanging += ComboBoxOptionalColumnsSelectedIndexChanging;
			comboBoxOptionalColumns.SelectedIndexChanged += ComboBoxOptionalColumnsSelectedIndexChanged;
			textBoxName.Validating +=TextBoxNameValidating;
			textBoxName.Validated +=TextBoxNameValidated;
            textBoxName.TextChanged += TextBoxNameTextChanged;
			buttonNew.Click += ButtonNewClick;
			buttonDelete.Click += ButtonDeleteClick;
        }

        void TextBoxNameTextChanged(object sender, EventArgs e)
        {
            ChangeOptionalColumnName();
        }

		private void ComboBoxOptionalColumnsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !IsWithinRange(e.NewIndex);
		}

		private void ComboBoxOptionalColumnsSelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
            textBoxName.TextChanged -= TextBoxNameTextChanged;
			SelectOptionalColumn();
            if (SelectedOptionalColumn != null)
		        ChangedInfo();
			Cursor.Current = Cursors.Default;
            textBoxName.TextChanged += TextBoxNameTextChanged;
		}        

		private void ButtonNewClick(object sender, EventArgs e)
		{
			if (SelectedOptionalColumn == null) return;
			Cursor.Current = Cursors.WaitCursor;

			AddNewOptionalColumn();

			Cursor.Current = Cursors.Default;
		}

		private void ButtonDeleteClick(object sender, EventArgs e)
		{
			if (SelectedOptionalColumn == null) return;
			var text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteOptionalColumn,
				SelectedOptionalColumn.Name
				);

			var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

			var response = ViewBase.ShowConfirmationMessage(text, caption);

			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;

			DeleteOptionalColumn();

			Cursor.Current = Cursors.Default;
		}

		private void TextBoxNameValidating(object sender, CancelEventArgs e)
		{
			if (SelectedOptionalColumn != null)
			{
				e.Cancel = !ValidatOptionalColumn();
			}
		}

		private void TextBoxNameValidated(object sender, EventArgs e)
		{
			ChangeOptionalColumnName();
		}

		private void SetColors()
		{
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
        }

        private IOptionalColumn CreateOptionalColumn()
        {
			var description = PageHelper.CreateNewName(_optionalColumnList, "Name", Resources.NewOptionalColumn);

            IOptionalColumn newOptionalColumn = new OptionalColumn(description.Name) { TableName = PersonTableName };
			Repository.Add(newOptionalColumn);

			return newOptionalColumn;
        }

        private void AddNewOptionalColumn()
        {
			_optionalColumnList.Add(CreateOptionalColumn());
			LoadOptionalColumns();
			comboBoxOptionalColumns.SelectedIndex = LastItemIndex;
            textBoxName.Focus();
            textBoxName.SelectAll();
        }

        private void DeleteOptionalColumn()
        {
        	if (SelectedOptionalColumn == null) return;
        	Repository.Remove(SelectedOptionalColumn);
        	_optionalColumnList.Remove(SelectedOptionalColumn);

        	LoadOptionalColumns();
        }

        private void LoadOptionalColumns()
        {
        	if (Disposing) return;
        	if (_optionalColumnList == null)
        	{
        		_optionalColumnList = new List<IOptionalColumn>();
        		_optionalColumnList.AddRange(Repository.GetOptionalColumns<Person>());
        	}

        	if (_optionalColumnList.IsEmpty())
        	{
        		_optionalColumnList.Add(CreateOptionalColumn());
        	}

        	var selected = comboBoxOptionalColumns.SelectedIndex;
        	if (!IsWithinRange(selected))
        	{
        		selected = FirstItemIndex;
        	}

        	comboBoxOptionalColumns.DataSource = null;
        	comboBoxOptionalColumns.DisplayMember = "Name";
        	comboBoxOptionalColumns.DataSource = _optionalColumnList;

        	comboBoxOptionalColumns.SelectedIndex = selected;
        }

        private void SelectOptionalColumn()
        {
            if (SelectedOptionalColumn != null)
            {
                textBoxName.Text = SelectedOptionalColumn.Name;
            }
        }

        private void ChangeOptionalColumnName()
        {
        	if (SelectedOptionalColumn == null) return;
        	SelectedOptionalColumn.Name = textBoxName.Text;
        	comboBoxOptionalColumns.Invalidate();
        	LoadOptionalColumns();
        }

        private bool ValidatOptionalColumn()
        {
            var failed = string.IsNullOrEmpty(textBoxName.Text);
            if (failed)
            {
                textBoxName.Text = SelectedOptionalColumn.Name;
            }

            return !failed;
        }

        private bool IsWithinRange(int index)
        {
			return index > InvalidItemIndex && index < _optionalColumnList.Count && comboBoxOptionalColumns.DataSource != null;
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonDelete, Resources.DeleteOptionalColumn);
            toolTip1.SetToolTip(buttonNew, Resources.NewOptionalColumn);
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        public void LoadControl()
        {
            LoadOptionalColumns();
        }

        public void SaveChanges()
        {}

        public void Unload()
        {
            _optionalColumnList = null;
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.OptionalColumn);
        }

        public string TreeNode()
        {
            return Resources.OptionalColumn;
        }

    	public void OnShow()
    	{
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;
            Repository = new OptionalColumnRepository(UnitOfWork);
        }

        public void Persist()
        {}

        private void ChangedInfo()
        {
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
            var changed = _localizer.UpdatedByText(SelectedOptionalColumn, Resources.UpdatedByColon);
            autoLabelInfoAboutChanges.Text = changed;
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.OptionalColumns; }
        }
    }
}
