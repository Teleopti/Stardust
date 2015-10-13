namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    public class DrawPositionAndWidth : IDrawPositionAndWidth
    {
        public int DrawingWidth { get; set; }
        public float ProgressStartPosition { get; set; }
        public float NameColumnStartPosition { get; set; }
        public int RowIndex{get; set;}
        public int ColIndex{get;set;}
    }
}