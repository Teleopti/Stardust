using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Drawing;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class PersistConflictView : BaseRibbonForm, 
                                                IPersistConflictView
    {
        private readonly PersistConflictPresenter _presenter;

		public PersistConflictView(IScheduleDictionary scheduleDictionary,
								IEnumerable<IPersistConflict> conflicts,
								ICollection<IPersistableScheduleData> modifiedDataResult)
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
			var model = new PersistConflictModel(scheduleDictionary, conflicts, modifiedDataResult);
			_presenter = new PersistConflictPresenter(this, model);
		}

		public new PersistConflictDialogResult DialogResult { get; set; }

		private void PersistConflict_Load(object sender, EventArgs e)
		{
			_presenter.Initialize();
		}
		
		public void SetupGridControl(IEnumerable<PersistConflictData> conflictCollection)
		{
			gridControlConflict.RowCount = conflictCollection.Count();

			for (int n = 2; n < gridControlConflict.RowCount+1; n += 2)
			{
				gridControlConflict.RowStyles[n].Interior = new BrushInfo(ColorHelper.OfficeBlue);
			}
			ResizeColumns();
		}

		private void ResizeColumns()
		{
			int width = gridControlConflict.Width / 4;
			gridControlConflict.Cols.Size[1] = width;
			gridControlConflict.Cols.Size[2] = width;
			gridControlConflict.Cols.Size[3] = width;
			gridControlConflict.Cols.Size[4] = width;
		}

		public void CloseForm(PersistConflictDialogResult dialogResult)
		{
			DialogResult = dialogResult;
			Close();
		}

		private void btnUndo_Click(object sender, EventArgs e)
		{
			_presenter.OnDiscardMyChanges();
		}

		private void btnOverWrite_Click(object sender, EventArgs e)
		{
			_presenter.OnOverwriteServerChanges();
		}

		private void gridControlConflict_QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex == 0)
				e.Style.BaseStyle = "Header";
			else
				e.Style.CellType = "Static";
			e.Style.CellValue = _presenter.OnQueryCellInfo(e.RowIndex, e.ColIndex);
		}
		
		private void gridControlConflict_Resize(object sender, EventArgs e)
		{
			ResizeColumns();
		}	
	}
}
