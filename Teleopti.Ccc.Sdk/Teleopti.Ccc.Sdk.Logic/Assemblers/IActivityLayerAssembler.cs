using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IActivityLayerAssembler<TLayerType> : IAssembler<TLayerType,ActivityLayerDto>
        where TLayerType : ILayer<IActivity>
    {
        void SetCurrentPerson(IPerson person);
    }
}