namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public interface IRestrictionSummaryGrid
    {
        void KeepSelection(bool keep);
        void TipText(int rowIndex, int colIndex, string text);
        void SetSelections(int rowIndex, bool update);
        int HeaderCount { get; set; }
        int RowCount { get; set; }
        int ColCount { get; set; }
        int CurrentCellRowIndex { get;}
        void Invalidate();
    }
}