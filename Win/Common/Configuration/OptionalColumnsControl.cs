﻿using System;
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
		private const string personTableName = "Person";
		private const short itemDiffernce = 1;
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
		public OptionalColumnRepository Repository { get; private set; }
		public IUnitOfWork UnitOfWork { get; private set; }
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public int LastItemIndex
		{
			get { return comboBoxOptionalColumns.Items.Count - itemDiffernce; }
		}

		public IOptionalColumn SelectedOptionalColumn
		{
			get { return comboBoxOptionalColumns.SelectedItem as IOptionalColumn; }
		}

		public OptionalColumnsControl()
		{
			InitializeComponent();

			// Bind events.
			comboBoxOptionalColumns.SelectedIndexChanging += comboBoxOptionalColumnsSelectedIndexChanging;
			comboBoxOptionalColumns.SelectedIndexChanged += comboBoxOptionalColumnsSelectedIndexChanged;
			textBoxName.Validating +=textBoxNameValidating;
			textBoxName.Validated +=textBoxNameValidated;
			textBoxName.TextChanged += textBoxNameTextChanged;
			buttonNew.Click += buttonNewClick;
			buttonDelete.Click += buttonDeleteClick;
		}

		void textBoxNameTextChanged(object sender, EventArgs e)
		{
			changeOptionalColumnName();
		}

		private void comboBoxOptionalColumnsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		private void comboBoxOptionalColumnsSelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			textBoxName.TextChanged -= textBoxNameTextChanged;
			selectOptionalColumn();
			if (SelectedOptionalColumn != null)
				changedInfo();
			Cursor.Current = Cursors.Default;
			textBoxName.TextChanged += textBoxNameTextChanged;
		}        

		private void buttonNewClick(object sender, EventArgs e)
		{
			if (SelectedOptionalColumn == null) return;
			Cursor.Current = Cursors.WaitCursor;

			addNewOptionalColumn();

			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteClick(object sender, EventArgs e)
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

			deleteOptionalColumn();

			Cursor.Current = Cursors.Default;
		}

		private void textBoxNameValidating(object sender, CancelEventArgs e)
		{
			if (SelectedOptionalColumn != null)
			{
				e.Cancel = !validatOptionalColumn();
			}
		}

		private void textBoxNameValidated(object sender, EventArgs e)
		{
			changeOptionalColumnName();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		private IOptionalColumn createOptionalColumn()
		{
			var description = PageHelper.CreateNewName(_optionalColumnList, "Name", Resources.NewOptionalColumn);

			IOptionalColumn newOptionalColumn = new OptionalColumn(description.Name) { TableName = personTableName };
			Repository.Add(newOptionalColumn);

			return newOptionalColumn;
		}

		private void addNewOptionalColumn()
		{
			_optionalColumnList.Add(createOptionalColumn());
			loadOptionalColumns();
			comboBoxOptionalColumns.SelectedIndex = LastItemIndex;
			textBoxName.Focus();
			textBoxName.SelectAll();
		}

		private void deleteOptionalColumn()
		{
			if (SelectedOptionalColumn == null) return;
			Repository.Remove(SelectedOptionalColumn);
			_optionalColumnList.Remove(SelectedOptionalColumn);

			loadOptionalColumns();
		}

		private void loadOptionalColumns()
		{
			if (Disposing) return;
			if (_optionalColumnList == null)
			{
				_optionalColumnList = new List<IOptionalColumn>();
				_optionalColumnList.AddRange(Repository.GetOptionalColumns<Person>());
			}

			if (_optionalColumnList.IsEmpty())
			{
				_optionalColumnList.Add(createOptionalColumn());
			}

			var selected = comboBoxOptionalColumns.SelectedIndex;
			if (!isWithinRange(selected))
			{
				selected = firstItemIndex;
			}

			comboBoxOptionalColumns.DataSource = null;
			comboBoxOptionalColumns.DisplayMember = "Name";
			comboBoxOptionalColumns.DataSource = _optionalColumnList;

			comboBoxOptionalColumns.SelectedIndex = selected;
		}

		private void selectOptionalColumn()
		{
			if (SelectedOptionalColumn != null)
			{
				textBoxName.Text = SelectedOptionalColumn.Name;
			}
		}

		private void changeOptionalColumnName()
		{
			if (SelectedOptionalColumn == null) return;
			SelectedOptionalColumn.Name = textBoxName.Text;
			comboBoxOptionalColumns.Invalidate();
			loadOptionalColumns();
		}

		private bool validatOptionalColumn()
		{
			var failed = string.IsNullOrEmpty(textBoxName.Text);
			if (failed)
			{
				textBoxName.Text = SelectedOptionalColumn.Name;
			}

			return !failed;
		}

		private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _optionalColumnList.Count && comboBoxOptionalColumns.DataSource != null;
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDelete, Resources.DeleteOptionalColumn);
			toolTip1.SetToolTip(buttonNew, Resources.NewOptionalColumn);
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void LoadControl()
		{
			loadOptionalColumns();
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

		private void changedInfo()
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
