﻿using System;
using System.ComponentModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionGrid : GridControl, IAgentRestrictionsView
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsModel _model;
		private IAgentRestrictionsWarningDrawer _warningDrawer;
		
		public AgentRestrictionGrid()
		{
			InitializeComponent();
			InitializeGrid();
		}

		public AgentRestrictionGrid(IContainer container)
		{
			if(container == null) throw new ArgumentNullException("container");
			container.Add(this);
			InitializeComponent();
			InitializeGrid();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeGrid()
		{
			_warningDrawer = new AgentRestrictionsWarningDrawer();
			_model = new AgentRestrictionsModel();
			_presenter = new AgentRestrictionsPresenter(this, _model, _warningDrawer);
			

			ResetVolatileData();
			GridHelper.GridStyle(this);
			InitializeHeaders();
			QueryColCount += GridQueryColCount;
			QueryRowCount += GridQueryRowCount;
			QueryCellInfo += GridQueryCellInfo;
			CellDrawn += GridCellDrawn;
			SelectionChanging += GridSelectionChanging;
			SelectionChanged += GridSelectionChanged;

			if (!CellModels.ContainsKey("NumericReadOnlyCellModel")) CellModels.Add("NumericReadOnlyCellModel",new NumericReadOnlyCellModel(Model) {NumberOfDecimals = 0});
			if (!CellModels.ContainsKey("TimeSpan")) CellModels.Add("TimeSpan", new TimeSpanLongHourMinutesStaticCellModel(Model));	
		}

		void GridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (e.Range.RangeType == GridRangeInfoType.Cells || Selections.Count > 1 || Selections.Ranges.Count > 0 && Selections.Ranges[0].Top != Selections.Ranges[0].Bottom)
			{
				var top = Selections.Ranges[0].Top;
				Selections.Clear();
				var rangelistTemp = new GridRangeInfoList {GridRangeInfo.Row(top)};
				Selections.SelectRange(rangelistTemp[0], true);							
			}
		}

		void GridSelectionChanging(object sender, GridSelectionChangingEventArgs e)
		{
			if (e.Reason == GridSelectionReason.MouseMove)
				e.Cancel = true;


			if (!e.Cancel && e.Range.Top <= 1 && e.Range.RangeType != GridRangeInfoType.Empty) 
				e.Cancel = true; 
		}


		private void InitializeHeaders()
		{
			Rows.HeaderCount = 1; // = 2 headers
			Cols.HeaderCount = 0; // = 1 header
		}

		public void MergeHeaders()
		{
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 0, 6));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 7, 0, 8));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 9, 0, 12));
		}

		public void LoadData()
		{
			_model.LoadData();
		}

		void GridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryColCount;
			e.Handled = true;
		}

		void GridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryRowCount;
			e.Handled = true;
		}

		void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			_presenter.GridQueryCellInfo(sender, e);
			e.Handled = true;
		}

		void GridCellDrawn(object sender, GridDrawCellEventArgs e)
		{
			_presenter.GridCellDrawn(e);
		}
	}
}
