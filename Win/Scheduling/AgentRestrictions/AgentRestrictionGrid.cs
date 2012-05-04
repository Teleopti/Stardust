using System;
using System.ComponentModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionGrid : GridControl
	{
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

		private void InitializeGrid()
		{
			GridHelper.GridStyle(this);
			InitializeHeaders();
		}

		private void InitializeHeaders()
		{
			Rows.HeaderCount = 1; // = 2 headers
			Cols.HeaderCount = 0; // = 1 header
			Model.Options.MergeCellsMode = GridMergeCellsMode.None;
			Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
		}
	}
}
