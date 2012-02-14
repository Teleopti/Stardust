using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IOvertimeLayerAssembler : IAssembler<IOvertimeShiftActivityLayer, OvertimeLayerDto>
    {
        void SetCurrentPerson(IPerson person);
    }
}