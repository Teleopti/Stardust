using System.Drawing;

namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    public interface IDrawProperties : IDrawPositionAndWidth
    {
        Graphics Graphics { get; }
        Rectangle Bounds { get; }
    }
}