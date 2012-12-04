using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class ShiftTradeAvailableCheckItem
    {
        public IPerson PersonFrom { get; set; }
        public IPerson PersonTo { get; set; }
        public DateOnly DateOnly { get; set; }
    }
}