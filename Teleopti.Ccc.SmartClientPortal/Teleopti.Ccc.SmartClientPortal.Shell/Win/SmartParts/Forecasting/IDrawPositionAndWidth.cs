namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    public interface IDrawPositionAndWidth
    {
        int DrawingWidth { get; }
        float ProgressStartPosition { get; }
        float NameColumnStartPosition { get; set; }
        int RowIndex { get; }
        int ColIndex { get; }
    }
}