using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public abstract class DropDownGridViewBase : GridViewBase
	{
		private const int DefaultGridInCellColumnIndex = 2;
		private const int DefaultParentChildGridMappingValue = 2;
		private const int DefaultParentGridLastColumnIndex = 0;
		private const int DefaultPushButtonColumnIndex = 1;
		private const int DefaultTotalHiddenColumnsInParentGrid = 1;
		private const int DefaultRowHeightValue = 20;
		private const int DefaultRenderingAddValue = 2;
		private const int DefaultColumnWidthAddValueInGrids = 10;
		private const int DefaultRowIndex = 1;

		public DropDownGridCreator GridCreator { get; private set; }

		public bool CanCopyChildRow { get; set; }

		public bool CanCopyRow { get; set; }

		public static int DefaultRowHeight
		{
			get
			{
				return DefaultRowHeightValue;
			}
		}

		public static int RenderingAddValue
		{
			get
			{
				return DefaultRenderingAddValue;
			}
		}

		public static int DefaultColumnWidthAddValue
		{
			get
			{
				return DefaultColumnWidthAddValueInGrids;
			}
		}

		public virtual int GridInCellColumnIndex
		{
			get
			{
				return DefaultGridInCellColumnIndex;
			}
		}

		public virtual int ParentGridLastColumnIndex
		{
			get
			{
				return DefaultParentGridLastColumnIndex;
			}
		}

		public virtual int TotalHiddenColumnsInParentGrid
		{
			get
			{
				return DefaultTotalHiddenColumnsInParentGrid;
			}
		}

		public virtual int ParentChildGridMappingValue
		{
			get
			{
				return DefaultParentChildGridMappingValue;
			}
		}

		public virtual int PushButtonColumnIndex
		{
			get
			{
				return DefaultPushButtonColumnIndex;
			}
		}

		protected DropDownGridViewBase(GridControl grid, FilteredPeopleHolder filteredPeopleHolder)
			: base(grid, filteredPeopleHolder)
		{
		}

		internal virtual void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
		}

		internal virtual void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.Style.HasPasswordChar) return;
			e.Style.CellTipText = e.Style.FormattedText;
		}

		internal virtual void ChildGridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
		{
		}

		internal virtual void ChildGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
		}

		internal virtual void ChildGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
		}

		internal virtual void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
		}

		internal virtual void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
		}

		internal virtual void ChildGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			var grid = sender as GridControl;

			if (grid != null)
			{
				CurrentGrid = grid;
			}

		}

		internal virtual void ChildGridClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
		}

		public virtual void BindDropDownGridEvents()
		{
			// Create Drop Down grid creator for child grid creation
			GridCreator = new DropDownGridCreator();

			GridCreator.DropDownGridQueryCellInfo += ChildGridQueryCellInfo;
			GridCreator.DropDownGridQueryRowCount += ChildGridQueryRowCount;
			GridCreator.DropDownGridQueryColCount += ChildGridQueryColCount;
			GridCreator.DropDownGridQueryRowHeight += ChildGridQueryRowHeight;
			GridCreator.DropDownGridQueryColWidth += ChildGridQueryColWidth;
			GridCreator.DropDownGridQuerySaveCellInfo += ChildGridQuerySaveCellInfo;
			GridCreator.DropDownGridSelectionChanged += ChildGridSelectionChanged;
			GridCreator.DropDownGridClipboardCanCopy += ChildGridClipboardCanCopy;
			GridCreator.DropDownGridClipboardPaste += ChildGridClipboardPaste;
		}

		protected override void Dispose(bool disposing)
		{
			if (GridCreator != null)
			{
				GridCreator.DropDownGridQueryCellInfo -= ChildGridQueryCellInfo;
				GridCreator.DropDownGridQueryRowCount -= ChildGridQueryRowCount;
				GridCreator.DropDownGridQueryColCount -= ChildGridQueryColCount;
				GridCreator.DropDownGridQueryRowHeight -= ChildGridQueryRowHeight;
				GridCreator.DropDownGridQueryColWidth -= ChildGridQueryColWidth;
				GridCreator.DropDownGridQuerySaveCellInfo -= ChildGridQuerySaveCellInfo;
				GridCreator.DropDownGridSelectionChanged -= ChildGridSelectionChanged;
				GridCreator.DropDownGridClipboardCanCopy -= ChildGridClipboardCanCopy;
				GridCreator.DropDownGridClipboardPaste -= ChildGridClipboardPaste;
				GridCreator.Dispose();
				GridCreator = null;
			}
			
			base.Dispose(disposing);
			
		}

		public void SetOutOfFocusFromCurrentCell()
		{
			int index = (Grid.Model.CurrentCellInfo == null) ? DefaultRowIndex :
															   Grid.Model.CurrentCellInfo.RowIndex;
			Grid.CurrentCell.MoveTo(index, DefaultParentGridLastColumnIndex);
			Grid.Selections.Clear(false);
		}
	}
}