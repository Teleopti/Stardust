using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public partial class AgentsNotPossibleToSchedule : BaseUserControl
	{
		private RestrictionNotAbleToBeScheduledReport _restrictionNotAbleToBeScheduledReport;
		private IEnumerable<IPerson> _selectedAgents;
		private DateOnlyPeriod _selectedDates;
		private IEnumerable<RestrictionsNotAbleToBeScheduledResult> _result;
		private SchedulerSplitters _parent;
		private AgentRestrictionsDetailView _detailView;
		private IRestrictionExtractor _restrictionExtractor;
		private readonly BlinkingToolStripButtonRenderer _blinker;
		private IDictionary<RestrictionNotAbleToBeScheduledReason, string> _translatedEnums;
		private const int numOfAgentsWithNoIssue = 50;

		public AgentsNotPossibleToSchedule()
		{
			InitializeComponent();
			_blinker = new BlinkingToolStripButtonRenderer(toolStrip1);
			if(!DesignMode)
				SetTexts();

			toolStripLabelManySelected.Text = Resources.NoAgentsToBeDisplayed;
			toolStripButtonShowNonIssued.Text = string.Format(Resources.ShowNoneIssuedAgents, numOfAgentsWithNoIssue);
		}

		public void InitAgentsNotPossibleToSchedule(RestrictionNotAbleToBeScheduledReport restrictionNotAbleToBeScheduledReport, SchedulerSplitters parent)
		{
			_restrictionNotAbleToBeScheduledReport = restrictionNotAbleToBeScheduledReport;
			_parent = parent;
			_restrictionExtractor = new RestrictionExtractor(new RestrictionCombiner(),
				new RestrictionRetrievalOperation());
			_translatedEnums = new Dictionary<RestrictionNotAbleToBeScheduledReason, string>();
			foreach (var keyValuePair in LanguageResourceHelper.TranslateEnumToList<RestrictionNotAbleToBeScheduledReason>())
			{
				_translatedEnums.Add(keyValuePair);
			}

			toolStripButtonShowNonIssued.Visible = true;
		}

		private void toolStripButtonRefreshClick(object sender, EventArgs e)
		{
			_blinker.BlinkButton(toolStripButtonRefresh, false);
			toolStripLabelManySelected.Visible = false;
			doCreate();
		}

		private void doCreate()
		{
			Cursor = Cursors.WaitCursor;
			toolStripButtonRefresh.Enabled = false;
			toolStripButtonShowNonIssued.Enabled = false;
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();
			listViewResult.ResumeLayout(true);
			backgroundWorker1.RunWorkerAsync();
		}

		public void SetSelected(IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedDates, AgentRestrictionsDetailView detailView)
		{
			toolStripButtonShowNonIssued.Enabled = true;
			_selectedAgents = selectedAgents;
			_selectedDates = selectedDates;
			_detailView = detailView;
			_detailView.ViewPasteCompleted += detailViewViewPasteCompleted;
			toolStripButtonShowNonIssued.Checked = false;
			autoCreateIfNotToManyAgents();
		}

		private void autoCreateIfNotToManyAgents()
		{
			toolStripLabelManySelected.Visible = false;
			if (_selectedAgents.Count() <= 300)
			{
				toolStripLabelManySelected.Visible = false;
				doCreate();
			}
			else
			{
				_blinker.BlinkButton(toolStripButtonRefresh, true);
				toolStripLabelManySelected.Text = string.Format(Resources.ManyAgentsAlert, _selectedAgents.Count());
				toolStripLabelManySelected.Visible = true;
			}
		}

		private void detailViewViewPasteCompleted(object sender, EventArgs e)
		{
			ReselectSelected();
		}

		public void ReselectSelected()
		{
			if (listViewResult.Items.Count == 0 || listViewResult.SelectedItems.Count == 0)
				return;

			if (listViewResult.SelectedItems.Count > 1)
				return;

			var selected = listViewResult.SelectedItems[0];
			var matrix = ((RestrictionsNotAbleToBeScheduledResult)selected.Tag).Matrix;
			_detailView.LoadDetails(matrix, _restrictionExtractor);
			_detailView.ViewGrid.Refresh();
			_detailView.InitializeGrid();
			_detailView.SelectDateIfExists(matrix.SchedulePeriod.DateOnlyPeriod.StartDate);
			_detailView.ViewGrid.Enabled = true;
			_detailView.UpdateShiftEditor();
			_detailView.ViewGrid.Cursor = Cursors.Default;
		}

		private void backgroundWorker1DoWork(object sender, DoWorkEventArgs e)
		{
			backgroundWorker1.ReportProgress(0, "XXAnalyzingRestrictions");
			_result = _restrictionNotAbleToBeScheduledReport.Create(_selectedDates,
				_selectedAgents, new BackgroundWorkerWrapper(backgroundWorker1));
		}

		private void backgroundWorker1ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_parent.OnRestrictionsNotAbleToBeScheduledProgress(e);
		}

		private void backgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}
			_parent.OnRestrictionsNotAbleToBeScheduledProgress(new ProgressChangedEventArgs(100, ""));

			var result = _result.Where(x => x.Reason != RestrictionNotAbleToBeScheduledReason.NoIssue && x.Reason != RestrictionNotAbleToBeScheduledReason.NoRestrictions);
			if (!result.Any())
			{
				toolStripLabelManySelected.Text = Resources.NoAgentsWithIssuesFound;
				toolStripLabelManySelected.Visible = true;
			}

			fillList(toolStripButtonShowNonIssued.Checked);

			toolStripButtonRefresh.Enabled = true;
			toolStripButtonShowNonIssued.Enabled = true;
			_detailView.ViewGrid.Enabled = true;
			_detailView.ViewGrid.Cursor = Cursors.Default;
			Cursor = Cursors.Default;
		}

		private void fillList(bool showAll)
		{
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();

			foreach (var restrictionsAbleToBeScheduledResult in _result.Where(x => x.Reason != RestrictionNotAbleToBeScheduledReason.NoIssue && x.Reason != RestrictionNotAbleToBeScheduledReason.NoRestrictions))
			{
				var item = new ListViewItem(restrictionsAbleToBeScheduledResult.Agent.Name.ToString())
				{
					Tag = restrictionsAbleToBeScheduledResult
				};
				if (!_translatedEnums.TryGetValue(restrictionsAbleToBeScheduledResult.Reason, out var translated))
				{
					translated = restrictionsAbleToBeScheduledResult.Reason.ToString();
				}

				item.SubItems.Add(translated);
				item.SubItems.Add(
					restrictionsAbleToBeScheduledResult.Period.ToShortDateString(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture));
				listViewResult.Items.Add(item);
			}

			if (showAll)
			{
				var result = _result.Where(x => x.Reason == RestrictionNotAbleToBeScheduledReason.NoIssue || x.Reason == RestrictionNotAbleToBeScheduledReason.NoRestrictions).Take(50).ToList();
				for (int i = 0; i < Math.Min(result.Count, 50); i++)
				{
					var item = new ListViewItem(result[i].Agent.Name.ToString())
					{
						Tag = result[i]
					};
					if (!_translatedEnums.TryGetValue(result[i].Reason, out var translated))
					{
						translated = result[i].Reason.ToString();
					}

					item.SubItems.Add(translated);
					item.SubItems.Add(
						result[i].Period.ToShortDateString(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture));
					listViewResult.Items.Add(item);
				}
			}
			
			listViewResult.ResumeLayout(true);
			if (listViewResult.Items.Count > 0)
				listViewResult.Items[0].Selected = true;
		}

		private void copyListBox()
		{
			var externalExceptionHandler = new ExternalExceptionHandler();
			externalExceptionHandler.AttemptToUseExternalResource(Clipboard.Clear);
			StringBuilder sb = new StringBuilder();
			foreach (ColumnHeader header in listViewResult.Columns)
			{
				sb.Append(header.Text + "\t");
			}
			sb.AppendLine();

			foreach (var item in listViewResult.SelectedItems)
			{
				if (item is ListViewItem l)
					foreach (ListViewItem.ListViewSubItem sub in l.SubItems)
						sb.Append(sub.Text + "\t");
				sb.AppendLine();
			}
			var dataObject = new DataObject();
			dataObject.SetData(DataFormats.Text, true, sb);
			externalExceptionHandler.AttemptToUseExternalResource(() => Clipboard.SetDataObject(dataObject, true));

		}

		private void selectAll()
		{
			listViewResult.SelectedIndexChanged -= listViewResultSelectedIndexChanged;
			listViewResult.SuspendLayout();
			foreach (ListViewItem item in listViewResult.Items)
			{
				item.Selected = true;
			}
			listViewResult.ResumeLayout(true);
			listViewResult.SelectedIndexChanged += listViewResultSelectedIndexChanged;
		}

		private void listViewResultSelectedIndexChanged(object sender, EventArgs e)
		{
			ReselectSelected();
		}

		private void listViewResultKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.C)
			{
				copyListBox();
			}

			if (e.Control && e.KeyCode == Keys.A)
			{
				selectAll();
			}
		}

		private void toolStripButtonShowNonIssuedClick(object sender, EventArgs e)
		{
			fillList(toolStripButtonShowNonIssued.Checked);
		}
	}
}
