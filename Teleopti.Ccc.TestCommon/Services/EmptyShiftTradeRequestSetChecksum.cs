using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
    public class EmptyShiftTradeRequestSetChecksum : IShiftTradeRequestSetChecksum
    {
        public void SetChecksum(IRequest request)
        {
        }
    }
}