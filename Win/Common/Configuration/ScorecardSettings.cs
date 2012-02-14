using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class ScorecardSettings : BaseUserControl, ISettingPage
    {
        private IList<IKeyPerformanceIndicator> _kpiList;
        private IList<IScorecard> _scorecardList;
        private bool _resetting;
        private IUnitOfWork _unitOfWork;

        private IScorecard SelectedScorecard
        {
            get
            {
                return (IScorecard)comboBoxAdvScorecard.SelectedItem;
            }
        }

        /// <summary>
        /// ctr
        /// </summary>
        public ScorecardSettings()
        {
            InitializeComponent();
            comboBoxAdvScorecard.SelectedIndexChanged += ComboBoxAdvScorecardSelectedIndexChanged;
            checkedListBoxKpi.ItemCheck += CheckedListBoxKpiItemCheck;
            textBoxScorecardName.Leave += TextBoxScorecardNameLeave;
            buttonNew.Click += ButtonNewClick;
            buttonAdvDeleteScorecard.Click += ButtonAdvDeleteScorecardClick;
            comboBoxAdvType.SelectedIndexChanged += ComboBoxAdvTypeSelectedIndexChanged;
        }

        private void ComboBoxAdvTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedScorecard == null) return;
            SelectedScorecard.Period = (IScorecardPeriod)comboBoxAdvType.SelectedItem;
        }

        private void TextBoxScorecardNameLeave(object sender, EventArgs e)
        {
            if (SelectedScorecard == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(textBoxScorecardName.Text))
            {
                textBoxScorecardName.Focus();
                return;
            }
            SelectedScorecard.Name = textBoxScorecardName.Text;
            IScorecard itemToBeSelected = SelectedScorecard;
            LoadScorecards();

            comboBoxAdvScorecard.SelectedIndex = _scorecardList.IndexOf(itemToBeSelected);
        }

        private void CheckedListBoxKpiItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (SelectedScorecard == null) return;
            if (_resetting) return;

            var theKpi = (IKeyPerformanceIndicator)checkedListBoxKpi.Items[e.Index];

            if (e.NewValue == CheckState.Unchecked)
                SelectedScorecard.RemoveKpi(theKpi);

            if (e.NewValue == CheckState.Checked && (SelectedScorecard.KeyPerformanceIndicatorCollection.Contains(theKpi) == false))
            {
                SelectedScorecard.AddKpi(theKpi);
            }
        }

        private void ComboBoxAdvScorecardSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedScorecard == null) return;
            SetCorrectKpiChecked();
            textBoxScorecardName.Text = SelectedScorecard.Name;

            comboBoxAdvType.SelectedValue = SelectedScorecard.Period.Id;
        }

        private void ButtonNewClick(object sender, EventArgs e)
        {
            IScorecard newScorecard = NewScorecard();
            _scorecardList.Add(newScorecard);

            LoadScorecards();
            comboBoxAdvScorecard.SelectedIndex = _scorecardList.IndexOf(newScorecard);
            ComboBoxAdvScorecardSelectedIndexChanged(this, EventArgs.Empty);
            //Selected the added Scorecard
        }

        private void ButtonAdvDeleteScorecardClick(object sender, EventArgs e)
        {
            if (SelectedScorecard == null) return;
            string text = string.Format(
                CurrentCulture,
                Resources.AreYouSureYouWantToDeleteItem,
                SelectedScorecard.Name
                );

            string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
            DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
            if (response != DialogResult.Yes) return;
            Cursor.Current = Cursors.WaitCursor;

            DeleteScorecard();

            Cursor.Current = Cursors.Default;
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();
            
            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            labelSubHeader3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader3.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
        }

        public void LoadControl()
        {
            LoadPeriods();
            LoadKpi();
            LoadScorecards();
            ComboBoxAdvScorecardSelectedIndexChanged(this, EventArgs.Empty);
        }

        public void SaveChanges()
        {
            if (_scorecardList.Count > 0 && ValidateData())
            {
                _scorecardList = null;
                LoadScorecards();
            }
            else
            {
                textBoxScorecardName.Text = string.Empty;
            }
        }

        public void Unload()
        {
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            _unitOfWork = value;
        }

        public void Persist()
        {
            _unitOfWork.PersistAll();
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(UserTexts.Resources.Scorecards, DefinedRaptorApplicationFunctionPaths.ManageScorecards);
        }

        public string TreeNode()
        {
            return UserTexts.Resources.Scorecards;
        }

    	public void OnShow()
    	{
    	}

    	public bool ValidateData()
        {
            string saveErrorCaption = UserTexts.Resources.SaveError;
            string blankDescriptionMessage = UserTexts.Resources.YouCannotHaveABlankDescription;

            if (string.IsNullOrEmpty(textBoxScorecardName.Text))
            {
                ShowMyErrorMessage(blankDescriptionMessage, saveErrorCaption);
                return false;
            }
            return true;
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonAdvDeleteScorecard, UserTexts.Resources.Delete);
            toolTip1.SetToolTip(buttonNew, UserTexts.Resources.AddNewScorecard);
        }

        private void DeleteScorecard()
        {
        	var scoreRep = new ScorecardRepository(_unitOfWork);
        	_scorecardList.Remove(SelectedScorecard);
        	scoreRep.Remove(SelectedScorecard);

        	//Make sure that teams with this scorecard gets disconnected, otherwise there will be a FK error!
        	var teamRepository = new TeamRepository(_unitOfWork);
        	using (_unitOfWork.DisableFilter(QueryFilter.Deleted))
        	{
        		var allTeams = teamRepository.LoadAll();
        		foreach (ITeam team in allTeams)
        		{
        			if (SelectedScorecard.Equals(team.Scorecard))
        			{
        				team.Scorecard = null;
        			}
        		}
        	}
                
        	if (_scorecardList.Count > 0)
        	{
        		LoadScorecards();
        		textBoxScorecardName.Text = SelectedScorecard.Name;
        		comboBoxAdvType.SelectedValue = SelectedScorecard.Period.Id;
        	}
        	else
        	{
        		comboBoxAdvScorecard.SelectedIndexChanged -= ComboBoxAdvScorecardSelectedIndexChanged;
        		comboBoxAdvScorecard.DataSource = null;
        		comboBoxAdvScorecard.SelectedIndexChanged += ComboBoxAdvScorecardSelectedIndexChanged;

        		textBoxScorecardName.Text = string.Empty;

        		for (int i = 0; i < checkedListBoxKpi.Items.Count; i++)
        		{
        			checkedListBoxKpi.SetItemChecked(i, false);
        		}
        	}
        }

        private void LoadPeriods()
        {
            if (comboBoxAdvType.DataSource != null) return;

            comboBoxAdvType.DisplayMember = "Name";
            comboBoxAdvType.ValueMember = "Id";
            comboBoxAdvType.DataSource = ScorecardPeriodService.ScorecardPeriodList().ToList();
        }

        private void LoadScorecards()
        {
            if (_scorecardList == null)
            {
                var scoreRep = new ScorecardRepository(_unitOfWork);
                _scorecardList = scoreRep.LoadAll();
            }
            if (_scorecardList.Count == 0)
                _scorecardList.Add(NewScorecard());

            _scorecardList = _scorecardList.OrderBy(p => p.Name).ToList();

            // Set same as before as selected if none was seelcted select the first
            int selected = comboBoxAdvScorecard.SelectedIndex;
            if (selected == -1) selected = 0;
            comboBoxAdvScorecard.SelectedIndexChanged -= ComboBoxAdvScorecardSelectedIndexChanged;
            comboBoxAdvScorecard.DataSource = null;
            comboBoxAdvScorecard.ValueMember = "";
            comboBoxAdvScorecard.DataSource = _scorecardList;
            comboBoxAdvScorecard.DisplayMember = "Name";
            comboBoxAdvScorecard.SelectedIndexChanged += ComboBoxAdvScorecardSelectedIndexChanged;

            if (selected >= comboBoxAdvScorecard.Items.Count)
                selected = comboBoxAdvScorecard.Items.Count - 1;

            comboBoxAdvScorecard.SelectedIndex = selected;

            SetCorrectKpiChecked();
        }

        private IScorecard NewScorecard()
        {
            var ret = new Scorecard();
            Description description = PageHelper.CreateNewName(_scorecardList, "Name", UserTexts.Resources.NewScorecard);
            ret.Name = description.Name;
            var scoreRep = new ScorecardRepository(_unitOfWork);
            scoreRep.Add(ret);
            return ret;
        }

        private void LoadKpi()
        {
        	if (_kpiList != null) return;
        	var kpiRep = new KpiRepository(_unitOfWork);
        	_kpiList = kpiRep.LoadAll();
        	_kpiList = _kpiList.OrderBy(p => p.Name).ToList();
        	checkedListBoxKpi.Items.Clear();
        	foreach (IKeyPerformanceIndicator item in _kpiList)
        	{
        		checkedListBoxKpi.Items.Add(item, false);
        	}
        }

        private void SetCorrectKpiChecked()
        {
            _resetting = true;
            checkedListBoxKpi.ClearSelected();
            for (int i = 0; i < checkedListBoxKpi.Items.Count; i++)
            {
                checkedListBoxKpi.SetItemChecked(i, false);
            }

            if (SelectedScorecard == null) return;

            int j = 0;
            var toBeChecked = new ArrayList();
            foreach (IKeyPerformanceIndicator item in checkedListBoxKpi.Items)
            {
                if (SelectedScorecard.KeyPerformanceIndicatorCollection.Contains(item))
                {
                    toBeChecked.Add(j);

                }
                j += 1;
            }
            foreach (int item in toBeChecked)
            {
                checkedListBoxKpi.SetItemChecked(item, true);
            }
            _resetting = false;
        }


        /// <summary>
        /// Shows Error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-04-08
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private void ShowMyErrorMessage(string message, string caption)
        {
            MessageBox.Show(
                string.Concat(message, "  "),
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                (RightToLeft == RightToLeft.Yes)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);
        }
        
        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }

        public ViewType ViewType
        {
            get { return ViewType.ScorecardSettings; }
        }
    }
}