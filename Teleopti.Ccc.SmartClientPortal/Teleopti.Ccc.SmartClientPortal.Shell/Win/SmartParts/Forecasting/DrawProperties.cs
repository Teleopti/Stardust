using System.Drawing;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public class DrawProperties : IDrawProperties
    {
        public int DrawingWidth { get; set; }
        public float ProgressStartPosition { get; set; }
        public Graphics Graphics { get; set; }
        public Rectangle Bounds { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public float NameColumnStartPosition { get; set; }
    }
}