using System;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsView
	{
		void MergeHeaders();
		void RefreshGrid();
		AgentRestrictionsDisplayRow CurrentDisplayRow { get; }
	}

	public interface IAgentRestrictionsWarningDrawer
	{
		void Draw(GridDrawCellEventArgs e,  IAgentRestrictionsModel model);
	}

	public interface IAgentRestrictionsDrawer
	{
		bool Draw(IAgentRestrictionsView view, GridQueryCellInfoEventArgs e, AgentRestrictionsDisplayRow agentRestrictionsDisplayRow);
	}

	public class AgentRestrictionsPresenter
	{
		private readonly IAgentRestrictionsView _view;
		private readonly IAgentRestrictionsModel _model;
		private readonly IAgentRestrictionsWarningDrawer _warningDrawer;
		private readonly IAgentRestrictionsDrawer _loadingDrawer;
		private readonly IAgentRestrictionsDrawer _notAvailableDrawer;
		private readonly IAgentRestrictionsDrawer _availableDrawer;
		private const int ColCount = 12;
		private const int HeaderCount = 1;
		private readonly bool[] _sortDirection;

		public AgentRestrictionsPresenter(IAgentRestrictionsView view, IAgentRestrictionsModel model, IAgentRestrictionsWarningDrawer warningDrawer, IAgentRestrictionsDrawer loadingDrawer, IAgentRestrictionsDrawer notAvailableDrawer, IAgentRestrictionsDrawer availableDrawer)
		{
			_view = view;
			_model = model;
			_warningDrawer = warningDrawer;
			_loadingDrawer = loadingDrawer;
			_notAvailableDrawer = notAvailableDrawer;
			_availableDrawer = availableDrawer;
			_sortDirection = Enumerable.Repeat(false, ColCount + 1).ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public int GridQueryColCount
		{
			get { return ColCount; }
		}

		public int GridQueryRowCount
		{
			get { return _model.DisplayRows.Count + HeaderCount; }
		}

		public void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if(e == null) throw new ArgumentNullException("e");

			HandleHeaderQuerys(e);
			HandleDisplayRowQuerys(e);
		}

		public void GridCellDrawn(GridDrawCellEventArgs e)
		{
			_warningDrawer.Draw(e, _model);	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private void HandleHeaderQuerys(GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex == 0)
			{
				if (e.ColIndex > 1 && e.ColIndex < 7) e.Style.Text = UserTexts.Resources.SchedulePeriod;
				if (e.ColIndex > 6 && e.ColIndex < 9) e.Style.Text = UserTexts.Resources.Schedule;
				if (e.ColIndex > 8 && e.ColIndex < 12) e.Style.Text = UserTexts.Resources.SchedulePlusRestrictions;
			}

			if (e.RowIndex == 1)
			{
				if (e.ColIndex == 1) e.Style.Text = UserTexts.Resources.Warnings;
				if (e.ColIndex == 2) e.Style.Text = UserTexts.Resources.Type;
				if (e.ColIndex == 3) e.Style.Text = UserTexts.Resources.From;
				if (e.ColIndex == 4) e.Style.Text = UserTexts.Resources.To;
				if (e.ColIndex == 5) e.Style.Text = UserTexts.Resources.ContractTargetTime;
				if (e.ColIndex == 6) e.Style.Text = UserTexts.Resources.DaysOff;
				if (e.ColIndex == 7) e.Style.Text = UserTexts.Resources.ContractTime;
				if (e.ColIndex == 8) e.Style.Text = UserTexts.Resources.DaysOff;
				if (e.ColIndex == 9) e.Style.Text = UserTexts.Resources.Min;
				if (e.ColIndex == 10) e.Style.Text = UserTexts.Resources.Max;
				if (e.ColIndex == 11) e.Style.Text = UserTexts.Resources.DaysOff;
				if (e.ColIndex == 12) e.Style.Text = UserTexts.Resources.Ok;
			}	
		}

		private void HandleDisplayRowQuerys(GridQueryCellInfoEventArgs e)
		{
			if(e.RowIndex > 1)
			{
				var displayRow = _model.DisplayRowFromRowIndex(e.RowIndex);
				if (displayRow == null) return;

				if (_loadingDrawer.Draw(_view, e, displayRow) && e.ColIndex > 0) return;
				if (_notAvailableDrawer.Draw(_view, e, displayRow) && e.ColIndex > 0) return;

				_availableDrawer.Draw(_view, e, displayRow);
				
			}
		}

		public void Sort(int column)
		{
			_sortDirection[column] = !_sortDirection[column];

			if (column == 0) _model.SortAgentName(_sortDirection[column]); 
			if (column == 1) _model.SortWarnings(_sortDirection[column]); 
			if (column == 2) _model.SortPeriodType(_sortDirection[column]); 
			if (column == 3) _model.SortStartDate(_sortDirection[column]); 
			if (column == 4) _model.SortEndDate(_sortDirection[column]);
			if (column == 5) _model.SortContractTargetTime(_sortDirection[column]); 
			if (column == 6) _model.SortTargetDayOffs(_sortDirection[column]); 
			if (column == 7) _model.SortContractCurrentTime(_sortDirection[column]);
			if (column == 8) _model.SortCurrentDayOffs(_sortDirection[column]); 
			if (column == 9) _model.SortMinimumPossibleTime(_sortDirection[column]); 
			if (column == 10) _model.SortMaximumPossibleTime(_sortDirection[column]); 
			if (column == 11) _model.SortScheduledAndRestrictionDayOffs(_sortDirection[column]);
			if (column == 12) _model.SortOk(_sortDirection[column]); 
			
			_view.RefreshGrid();
		}
	}
}
