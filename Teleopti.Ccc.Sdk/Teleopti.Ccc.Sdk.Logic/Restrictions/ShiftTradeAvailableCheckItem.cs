using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public interface IShiftTradeAvailableCheckItem
    {
        IPerson PersonFrom { get; set; }
        IPerson PersonTo { get; set; }
        DateOnly DateOnly { get; set; }
    }

    public class ShiftTradeAvailableCheckItem : IShiftTradeAvailableCheckItem
    {
        public IPerson PersonFrom { get; set; }
        public IPerson PersonTo { get; set; }
        public DateOnly DateOnly { get; set; }
    }
}