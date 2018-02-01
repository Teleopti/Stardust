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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public partial class AgentsNotPossibleToSchedule : BaseUserControl
	{
		private RestrictionNotAbleToBeScheduledReport _restrictionNotAbleToBeScheduledReport;
		private IEnumerable<IPerson> _selectedAgents;
		private DateOnly _selectedDate;
		private IEnumerable<RestrictionsNotAbleToBeScheduledResult> _result;
		private SchedulerSplitters _parent;
		private AgentRestrictionsDetailView _detailView;
		private IRestrictionExtractor _restrictionExtractor;
		private readonly BlinkingToolStripButtonRenderer _blinker;
		private IDictionary<RestrictionNotAbleToBeScheduledReason, string> _translatedEnums;

		public AgentsNotPossibleToSchedule()
		{
			InitializeComponent();
			_blinker = new BlinkingToolStripButtonRenderer(toolStrip1);
			if(!DesignMode)
				SetTexts();
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
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();
			listViewResult.ResumeLayout(true);
			backgroundWorker1.RunWorkerAsync();
		}

		public void SetSelected(IEnumerable<IPerson> selectedAgents, DateOnly selectedDate, AgentRestrictionsDetailView detailView)
		{
			_selectedAgents = selectedAgents;
			_selectedDate = selectedDate;
			_detailView = detailView;
			_detailView.ViewPasteCompleted += detailViewViewPasteCompleted;
			autoCreateIfNotToManyAgents();
		}

		private void autoCreateIfNotToManyAgents()
		{
			if (_selectedAgents.Count() <= 300)
			{
				toolStripLabelManySelected.Visible = false;
				doCreate();
			}
			else
			{
				_blinker.BlinkButton(toolStripButtonRefresh, true);
				toolStripLabelManySelected.Text = _selectedAgents.Count() + @" " + toolStripLabelManySelected.Text;
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

			var selected = listViewResult.SelectedItems[0];
			var matrix = ((RestrictionsNotAbleToBeScheduledResult)selected.Tag).Matrix;
			_detailView.LoadDetails(matrix, _restrictionExtractor);
			_detailView.TheGrid.Refresh();
			_detailView.InitializeGrid();
			_detailView.SelectDateIfExists(matrix.SchedulePeriod.DateOnlyPeriod.StartDate);
			_detailView.TheGrid.Enabled = true;
			_detailView.UpdateShiftEditor();
			_detailView.TheGrid.Cursor = Cursors.Default;
		}

		private void backgroundWorker1DoWork(object sender, DoWorkEventArgs e)
		{
			backgroundWorker1.ReportProgress(0, "XXAnalyzingRestrictions");
			_result = _restrictionNotAbleToBeScheduledReport.Create(_selectedDate,
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
			listViewResult.SuspendLayout();
			listViewResult.Items.Clear();

			foreach (var restrictionsAbleToBeScheduledResult in _result)
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
				item.SubItems.Add(restrictionsAbleToBeScheduledResult.Period.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
				listViewResult.Items.Add(item);
			}

			listViewResult.ResumeLayout(true);
			if (listViewResult.Items.Count > 0)
				listViewResult.Items[0].Selected = true;

			toolStripButtonRefresh.Enabled = true;
			_detailView.TheGrid.Enabled = true;
			_detailView.TheGrid.Cursor = Cursors.Default;
			Cursor = Cursors.Default;
		}

		private void copyListBox()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in listViewResult.SelectedItems)
			{
				if (item is ListViewItem l)
					foreach (ListViewItem.ListViewSubItem sub in l.SubItems)
						sb.Append(sub.Text + "\t");
				sb.AppendLine();
			}
			Clipboard.SetDataObject(sb.ToString());

		}

		private void selectAll()
		{
			listViewResult.SuspendLayout();
			foreach (ListViewItem item in listViewResult.Items)
			{
				item.Selected = true;
			}
			listViewResult.ResumeLayout(true);
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
	}
}
