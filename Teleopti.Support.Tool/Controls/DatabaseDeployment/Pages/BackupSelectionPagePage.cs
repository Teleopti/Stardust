using System;
using System.Linq;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	public partial class BackupSelectionPagePage : SelectionPage
	{
		public BackupFileType BackupFileType { get; private set; }
		private readonly DatabaseDeploymentModel _model;
		private DatabaseSourceType _selectedControlType;

		public BackupSelectionPagePage()
		{
			InitializeComponent();
			setFromSourceTypeSelection(DatabaseSourceType.FromArchive);
		}

		public BackupSelectionPagePage(BackupFileType backupFileType, DatabaseDeploymentModel model)
			: this()
		{
			// ReSharper disable LocalizableElement
			BackupFileType = backupFileType;
			_model = model;
			switch (BackupFileType)
			{
				case BackupFileType.TeleoptiCCC7:
					labelBackupFileSelection.Text = labelBackupFileSelection.Text + " for application database";
					break;
				case BackupFileType.TeleoptiAnalytics:
					labelBackupFileSelection.Text = labelBackupFileSelection.Text + " for analytics database";
					break;
				case BackupFileType.TeleoptiCCCAgg:
					labelBackupFileSelection.Text = labelBackupFileSelection.Text + " for agg database";
					break;
			}
			// ReSharper restore LocalizableElement
		}

		private void setFromSourceTypeSelection(DatabaseSourceType type)
		{
			_selectedControlType = type;
			comboBoxArchivedFile.Enabled = (type == DatabaseSourceType.FromArchive);
			textBoxExistingBackupFile.Enabled = (type == DatabaseSourceType.ExistingFile);
			buttonBrowseBackupFile.Enabled = (type == DatabaseSourceType.ExistingFile);
			comboBoxExistingDatabase.Enabled = (type == DatabaseSourceType.ExistingDatabase);
			textBoxRestoredDatabaseName.Enabled = (type != DatabaseSourceType.ExistingDatabase);
		}

		private void radioButtonFromArchive_CheckedChanged(object sender, EventArgs e)
		{
			setFromSourceTypeSelection(DatabaseSourceType.FromArchive);
			if (((RadioButton) sender).Checked)
			{
				selectionChanged(sender, e);
			}
		}

		private void radioButtonExistingBackupFile_CheckedChanged(object sender, EventArgs e)
		{
			setFromSourceTypeSelection(DatabaseSourceType.ExistingFile);
			if (((RadioButton)sender).Checked)
				selectionChanged(sender, e);
		}

		private void radioButtonUseExistingDatabase_CheckedChanged(object sender, EventArgs e)
		{
			setFromSourceTypeSelection(DatabaseSourceType.ExistingDatabase);
			loadDatabases();
			if (((RadioButton)sender).Checked)
				selectionChanged(sender, e);
		}

		private void selectionChanged(object sender, EventArgs eventArgs)
		{
			if (sender == comboBoxArchivedFile)
				setSuggestionName(comboBoxArchivedFile.Text);
			else if (sender == textBoxExistingBackupFile)
				setSuggestionName(textBoxExistingBackupFile.Text);

			var isValid = changeIsValid();
			triggerHasValidInput(isValid);
		}

        public override bool ContentIsValid()
        {
            return changeIsValid();
        }

		private bool changeIsValid()
		{
			var selection = getDatafromControl();
			if (selection == null) return false;

			if (selection.DatabaseFromSourceType != DatabaseSourceType.ExistingDatabase)
				return !string.IsNullOrWhiteSpace(selection.DatabasePath) && !string.IsNullOrWhiteSpace(selection.DatabaseName);
			return !string.IsNullOrWhiteSpace(selection.DatabasePath);
		}

		private void setSuggestionName(string text)
		{
			var indexOfLastSlash = text.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
			var indexOfBak = text.IndexOf(".bak", StringComparison.OrdinalIgnoreCase);

			if (indexOfLastSlash != -1 && indexOfBak != -1)
				textBoxRestoredDatabaseName.Text = text.Substring(indexOfLastSlash + 1, indexOfBak - indexOfLastSlash - 1);
			else if (indexOfLastSlash != -1 && indexOfBak == -1)
				textBoxRestoredDatabaseName.Text = text.Substring(indexOfLastSlash + 1, text.Length - indexOfLastSlash - 1);
			else if (indexOfLastSlash == -1 && indexOfBak != -1)
				textBoxRestoredDatabaseName.Text = text.Substring(0, indexOfBak);
			else
				textBoxRestoredDatabaseName.Text = text;
		}

		private void loadDatabases()
		{
			if (comboBoxExistingDatabase.Items.Count != 0) return;

			Cursor = Cursors.WaitCursor;
			var dbs = _model.Helper.Databases;
			comboBoxExistingDatabase.Items.AddRange(dbs.Cast<object>().ToArray());
			Cursor = Cursors.Default;
		}

		public override void GetData()
		{
			switch (BackupFileType)
			{
				case BackupFileType.TeleoptiAnalytics:
					_model.SelectedAnalyticsDatabase = getDatafromControl();
					_model.SelectedAnalyticsDatabase.DatabaseSource = BackupFileType.TeleoptiAnalytics;
					break;
				case BackupFileType.TeleoptiCCC7:
					_model.SelectedAppDatabase = getDatafromControl();
					_model.SelectedAppDatabase.DatabaseSource = BackupFileType.TeleoptiCCC7;
					break;
				case BackupFileType.TeleoptiCCCAgg:
					_model.SelectedAggDatabase = getDatafromControl();
					_model.SelectedAggDatabase.DatabaseSource = BackupFileType.TeleoptiCCCAgg;
					break;
			}
		}

		private DatabaseInfo getDatafromControl()
		{
			switch (_selectedControlType)
			{
				case DatabaseSourceType.ExistingDatabase:
					return new DatabaseInfo
						{
							DatabasePath = comboBoxExistingDatabase.Text,
							DatabaseFromSourceType = DatabaseSourceType.ExistingDatabase,
							DatabaseName = comboBoxExistingDatabase.Text
						};
				case DatabaseSourceType.ExistingFile:
					return new DatabaseInfo
						{
							DatabasePath = textBoxExistingBackupFile.Text,
							DatabaseFromSourceType = DatabaseSourceType.ExistingFile,
							DatabaseName = textBoxRestoredDatabaseName.Text
						};
				case DatabaseSourceType.FromArchive:
					return new DatabaseInfo
						{
							DatabasePath = comboBoxArchivedFile.Text,
							DatabaseFromSourceType = DatabaseSourceType.FromArchive,
							DatabaseName = textBoxRestoredDatabaseName.Text
						};
			}
			return null;
		}

		public override void SetData()
		{
			textBoxRestoredDatabaseName.Text = string.Empty;
			comboBoxArchivedFile.Text = string.Empty;
			comboBoxArchivedFile.Items.Clear();
			comboBoxArchivedFile.Items.Add("");
			comboBoxArchivedFile.Items.AddRange(_model.SelectableDatabaseFiles.Cast<object>().ToArray());

			var suggestion = _model.GetSuggestions()
								   .Where(a => a != null)
								   .FirstOrDefault(a => a.DatabaseSource == BackupFileType);
			if (suggestion == null)
			{
				radioButtonUseExistingDatabase.Checked = true;
				return;
			}

			methodOfShame();

			var path = suggestion.DatabasePath;
			comboBoxArchivedFile.Text = suggestion.DatabasePath;
            int startOfDotBak = path.IndexOf(".bak", StringComparison.OrdinalIgnoreCase);
            if (startOfDotBak == -1)
            {
                startOfDotBak = path.Length;
            }
			textBoxRestoredDatabaseName.Text = path.Substring(0, startOfDotBak);
		}

		private void methodOfShame()
		{
			switch (BackupFileType)
			{
				case BackupFileType.TeleoptiCCC7:
					if (_model.SelectedAppDatabase != null)
					{
						radioButtonFromArchive.Checked = (_model.SelectedAppDatabase.DatabaseFromSourceType ==
														  DatabaseSourceType.FromArchive);
						radioButtonExistingBackupFile.Checked = (_model.SelectedAppDatabase.DatabaseFromSourceType ==
																 DatabaseSourceType.ExistingDatabase);
						radioButtonUseExistingDatabase.Checked = (_model.SelectedAppDatabase.DatabaseFromSourceType ==
																  DatabaseSourceType.ExistingDatabase);
					}
					break;
				case BackupFileType.TeleoptiAnalytics:
					if (_model.SelectedAnalyticsDatabase != null)
					{
						radioButtonFromArchive.Checked = (_model.SelectedAnalyticsDatabase.DatabaseFromSourceType ==
														  DatabaseSourceType.FromArchive);
						radioButtonExistingBackupFile.Checked = (_model.SelectedAnalyticsDatabase.DatabaseFromSourceType ==
																 DatabaseSourceType.ExistingDatabase);
						radioButtonUseExistingDatabase.Checked = (_model.SelectedAnalyticsDatabase.DatabaseFromSourceType ==
																  DatabaseSourceType.ExistingDatabase);
					}
					break;
				case BackupFileType.TeleoptiCCCAgg:
					if (_model.SelectedAggDatabase != null)
					{
						radioButtonFromArchive.Checked = (_model.SelectedAggDatabase.DatabaseFromSourceType ==
														  DatabaseSourceType.FromArchive);
						radioButtonExistingBackupFile.Checked = (_model.SelectedAggDatabase.DatabaseFromSourceType ==
																 DatabaseSourceType.ExistingDatabase);
						radioButtonUseExistingDatabase.Checked = (_model.SelectedAggDatabase.DatabaseFromSourceType ==
																  DatabaseSourceType.ExistingDatabase);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		private void backupSelectionPagePage_ParentChanged(object sender, EventArgs e)
		{
			var isValid = changeIsValid();
			triggerHasValidInput(isValid);
			if (!_model.SkippedFirstStep) return;

			radioButtonExistingBackupFile.Checked = true;
			radioButtonFromArchive.Enabled = false;
		}

		private void buttonBrowseBackupFile_Click(object sender, EventArgs e)
		{
			if (openFileDialogExistingBackup.ShowDialog(this) != DialogResult.OK)
				return;

			textBoxExistingBackupFile.Text = openFileDialogExistingBackup.FileName;
		}
	}
}
