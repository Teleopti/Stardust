using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite;
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
		private readonly BindingList<IScorecard> _scorecardList = new BindingList<IScorecard>();
		private bool _resetting;
		private IUnitOfWork _unitOfWork;

		private IScorecard SelectedScorecard
		{
			get
			{
				return (IScorecard)comboBoxAdvScorecard.SelectedValue;
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
			BindScorecard(); // update the name field in the scorecard combo
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

			comboBoxAdvScorecard.SelectedItem = newScorecard;
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

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
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

			BindScorecard();
			LoadScorecards();
		}

		private void BindScorecard()
		{
			comboBoxAdvScorecard.DataSource = null;
			comboBoxAdvScorecard.ValueMember = "";
			comboBoxAdvScorecard.DataSource = _scorecardList;
			comboBoxAdvScorecard.DisplayMember = "Name";
		}

		public void SaveChanges()
		{
			if (_scorecardList.Count <= 0 || !ValidateData())
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
			return new TreeFamily(Resources.Scorecards, DefinedRaptorApplicationFunctionPaths.ManageScorecards);
		}

		public string TreeNode()
		{
			return Resources.Scorecards;
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

			var scoreRep = new ScorecardRepository(_unitOfWork);
			scoreRep.Remove(SelectedScorecard);

			_scorecardList.Remove(SelectedScorecard);

			if (_scorecardList.Count > 0)
			{
				textBoxScorecardName.Text = SelectedScorecard.Name;
				comboBoxAdvType.SelectedValue = SelectedScorecard.Period.Id;
			}
			else
			{
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
				var scoreRep = new ScorecardRepository(_unitOfWork);
				_scorecardList.Clear();
				_scorecardList.AddRange(scoreRep.LoadAll().OrderBy(p => p.Name).ToList());
		   
			if (_scorecardList.Count == 0)
				_scorecardList.Add(NewScorecard());

				comboBoxAdvScorecard.SelectedIndex = -1;
				comboBoxAdvScorecard.SelectedIndex = 0;				

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private static void ShowMyErrorMessage(string message, string caption)
		{
			ViewBase.ShowErrorMessage(message, caption);
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