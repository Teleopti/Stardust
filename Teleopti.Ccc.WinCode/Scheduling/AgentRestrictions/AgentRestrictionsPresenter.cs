using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAgentRestrictionsView
	{
		void MergeHeaders();
		void LoadData();
	}

	public interface IAgentRestrictionsWarningDrawer
	{
		void Draw(GridDrawCellEventArgs e, IAgentRestrictionsDisplayRow agentRestrictionsDisplayRow);
	}

	public class AgentRestrictionsPresenter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private IAgentRestrictionsView _view;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IAgentRestrictionsModel _model;
		private IAgentRestrictionsWarningDrawer _warningDrawer;
		private const int ColCount = 12;
		private const int HeaderCount = 1;

		public AgentRestrictionsPresenter(IAgentRestrictionsView view, IAgentRestrictionsModel model, IAgentRestrictionsWarningDrawer warningDrawer)
		{
			_view = view;
			_model = model;
			_warningDrawer = warningDrawer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public int GridQueryColCount
		{
			get { return ColCount; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
			//if warnings
			_warningDrawer.Draw(e, null);
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
				if (e.ColIndex == 0) e.Style.Text = UserTexts.Resources.Name;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private void HandleDisplayRowQuerys(GridQueryCellInfoEventArgs e)
		{
			if(e.RowIndex > 1)
			{
				//Name
				if (e.ColIndex == 0)
				{
					e.Style.CellType = "Header";
					//e.Style.CellValue = Model.
				}

				//Warnings
				if (e.ColIndex == 1)
				{
					e.Style.CellType = "NumericReadOnlyCellModel";
					//e.Style.CellValue = Model.
				}

				//Type
				if (e.ColIndex == 2)
				{
					e.Style.CellType = "Static";
					//e.Style.CellValue = Model.
				}

				//From
				if (e.ColIndex == 3)
				{
					e.Style.CellType = "Static";
					//e.Style.CellValue = Model.
				}

				//To
				if (e.ColIndex == 4)
				{
					e.Style.CellType = "Static";
					//e.Style.CellValue = Model.
				}

				//Contract Target Time
				if (e.ColIndex == 5)
				{
					e.Style.CellType = "TimeSpan";
					//e.Style.CellValue = Model.
				}

				//Days Off (Schedule Period)
				if (e.ColIndex == 6)
				{
					e.Style.CellType = "NumericReadOnlyCellModel";
					//e.Style.CellValue = Model.
				}

				//Contract time
				if (e.ColIndex == 7)
				{
					e.Style.CellType = "TimeSpan";
					//e.Style.CellValue = Model.
				}

				//Days off (Schedule)
				if (e.ColIndex == 8)
				{
					e.Style.CellType = "NumericReadOnlyCellModel";
					//e.Style.CellValue = Model.
				}

				//Min
				if (e.ColIndex == 9)
				{
					e.Style.CellType = "TimeSpan";
					//e.Style.CellValue = Model.
				}

				//Max
				if (e.ColIndex == 10)
				{
					e.Style.CellType = "TimeSpan";
					//e.Style.CellValue = Model.
				}

				//Days Off (Schedule + Restrictions)
				if (e.ColIndex == 11)
				{
					e.Style.CellType = "NumericReadOnlyCellModel";
					//e.Style.CellValue = Model.
				}

				//Ok
				if (e.ColIndex == 12)
				{
					e.Style.CellType = "Static";
					//e.Style.CellValue = Model.
				}
			}
		}
	}
}
