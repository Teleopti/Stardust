using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IOvertimeLayerAssembler : IAssembler<OvertimeShiftLayer, OvertimeLayerDto>
    {
        void SetCurrentPerson(IPerson person);
    }
}