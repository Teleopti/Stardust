using System;
using System.Linq;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	public partial class ArchivedSetSelectionPagePage : SelectionPage
	{
		private readonly DatabaseDeploymentModel _model;

        public delegate void SkipThisStepDelegate();
        public event SkipThisStepDelegate SkipThisStep;

		public ArchivedSetSelectionPagePage(DatabaseDeploymentModel model)
		{
			_model = model;
			InitializeComponent();
		}

		public override void GetData()
		{
			_model.ZipFilePath = textBoxArchivedFileSet.Text;
		}

		public override void SetData() { }

		private void buttonBrowse_Click(object sender, EventArgs e)
		{
			if (openFileDialogArchivedFile.ShowDialog(this) != DialogResult.OK) return;

			Cursor = Cursors.WaitCursor;
			textBoxArchivedFileSet.Text = openFileDialogArchivedFile.FileName;
		}

        public override bool ContentIsValid()
        {
            return checkValidInput();
        }

		private bool checkValidInput()
		{
			// TODO: Behöver vi andra sätt att validera?
			return !string.IsNullOrEmpty(textBoxArchivedFileSet.Text) &&
				(textBoxArchivedFileSet.Text.EndsWith(".zip") ||
				textBoxArchivedFileSet.Text.EndsWith(".7z"));
		}

		private void textBoxArchivedFileSet_TextChanged(object sender, EventArgs e)
		{
			// ReSharper disable LocalizableElement
			var isValid = checkValidInput();
			triggerHasValidInput(isValid);
			if (!isValid) return;

			try
			{
				textBoxContents.Text = "Contents:" + Environment.NewLine;
                SevenZip.SevenZipBase.SetLibraryPath(_model.SettingsInRegistry.LocationOf7zDll);
                SevenZip.SevenZipExtractor extractor = new SevenZip.SevenZipExtractor(textBoxArchivedFileSet.Text);
                extractor.ArchiveFileNames.ToList().ForEach(c => textBoxContents.Text += "    " + c + Environment.NewLine + Environment.NewLine);
				//var zipfile = new ZipFile(textBoxArchivedFileSet.Text);
				//zipfile.EntryFileNames.ToList().ForEach(c => textBoxContents.Text += "    " + c + Environment.NewLine + Environment.NewLine);
			}
			catch (Exception exception)
			{
				MessageBox.Show(this, exception.Message);
			}
			// ReSharper restore LocalizableElement
			Cursor = Cursors.Default;
		}

        //private void onParentChanged(object sender, EventArgs eventArgs)
        //{
        //    var isValid = checkValidInput();
        //    triggerHasValidInput(isValid);
        //}

        private void linkLabelSkipThis_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SkipThisStep();
        }

	}
}
