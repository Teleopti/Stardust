using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment
{
    public partial class DatabaseDeployMain : UserControl
    {
	    private readonly MainForm _mainForm;
	    private IList<SelectionPage> _selectionPages;
        private SelectionPage _activeControl;
	    private readonly DatabaseDeploymentModel _model;

        public DatabaseDeployMain(MainForm mainForm, DBHelper helper, SettingsInRegistry settingsInRegistry)
        {
	        _mainForm = mainForm;
	        InitializeComponent();

	        _model = new DatabaseDeploymentModel {Helper = helper, SettingsInRegistry = settingsInRegistry};
	        inititalizeSelectionPages();
        }

        private void inititalizeSelectionPages()
        {
            var asspp = new ArchivedSetSelectionPagePage( _model);
            asspp.SkipThisStep += asspp_SkipThisStep;

	        var nHibernateSessionFilePage = new NHibernateSessionFilePage(_model);
			nHibernateSessionFilePage.Deploy += nhibsfp_Deploy;

            var configure7ZipDllLocation = new Configure7ZipDllLocation(_model);
	        _selectionPages = new List<SelectionPage>();
            if (!configure7ZipDllLocation.ContentIsValid())
            {
                _selectionPages.Add(configure7ZipDllLocation);
            }
            _selectionPages.Add(asspp);
            _selectionPages.Add(new BackupSelectionPagePage(BackupFileType.TeleoptiCCC7, _model));
            _selectionPages.Add(new BackupSelectionPagePage(BackupFileType.TeleoptiAnalytics, _model));
            _selectionPages.Add(new BackupSelectionPagePage(BackupFileType.TeleoptiCCCAgg, _model));
            _selectionPages.Add(nHibernateSessionFilePage);

	        _activeControl = _selectionPages[0];

	        foreach (var bsp in _selectionPages)
		        bsp.hasValidInput += bsp_HasValidInput;

	        setPage(_activeControl);
        }

		void nhibsfp_Deploy()
		{
			buttonNext.Visible = false;
			buttonBack.Visible = false;
			var progressPage = new ProgressPage(_model);
			setPage(progressPage);
		}

        void asspp_SkipThisStep()
        {
            _model.SkippedFirstStep = true;
            goToNextPage();
        }

	    private void bsp_HasValidInput(bool isValid)
        {
            buttonNext.Enabled = isValid && !(_activeControl is NHibernateSessionFilePage);
        }

        private void setPage(SelectionPage selectionPage)
        {
            SelectionPage sp = selectionPage as SelectionPage;
            if (sp != null)
            {
                buttonNext.Enabled = sp.ContentIsValid();
            }
            else
            {
                buttonNext.Enabled = !(_activeControl is NHibernateSessionFilePage);
            }
            panelPageContainer.Controls.Clear();
            _activeControl = selectionPage;
            _activeControl.Dock = DockStyle.Fill;
            panelPageContainer.Controls.Add(_activeControl);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            _model.SkippedFirstStep = false;
            goToNextPage();
        }

        private void goToNextPage()
        {
            transformData();
            var indexOfActive = _selectionPages.IndexOf(selectionPage());
            _activeControl = _selectionPages[indexOfActive + 1];
            setPage(_activeControl);
			selectionPage().SetData();
        }

	    private void transformData()
	    {
			var selctionPage = selectionPage();
			selctionPage.GetData();
			if (selctionPage is ArchivedSetSelectionPagePage)
			{
                _model.SelectableDatabaseFiles.Clear();
                if (!string.IsNullOrEmpty(_model.ZipFilePath))
                {
                    SevenZip.SevenZipExtractor extractor = new SevenZip.SevenZipExtractor(_model.ZipFilePath);
                    //var zipFile = new ZipFile(_model.ZipFilePath);
                    //_model.SelectableDatabaseFiles.AddRange(zipFile.EntryFileNames);
                    _model.SelectableDatabaseFiles.AddRange(extractor.ArchiveFileNames);
                }
                DatabaseDeployHelper.MapSuggestions(_model);
			}
			else if (selctionPage is BackupSelectionPagePage)
			{
			}
	    }

	    private void buttonBack_Click(object sender, EventArgs e)
	    {
            var indexOfActive = _selectionPages.IndexOf(selectionPage());	
		    if (indexOfActive == 0)
		    {
				_mainForm.ShowPTracks();
			    return;
		    }
			selectionPage().GetData();
            _activeControl = _selectionPages[indexOfActive - 1];
            setPage(_activeControl);
        }

		private SelectionPage selectionPage()
		{
			return (SelectionPage)_activeControl;
        }

        private void linkLabelGoHome_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _mainForm.ShowPTracks();
        }
    }
}
