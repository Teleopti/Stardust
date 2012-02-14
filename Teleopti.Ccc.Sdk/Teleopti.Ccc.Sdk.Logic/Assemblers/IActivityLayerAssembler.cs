using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IActivityLayerAssembler<TLayerType> : IAssembler<TLayerType,ActivityLayerDto>
        where TLayerType : ILayer<IActivity>
    {
        void SetCurrentPerson(IPerson person);
    }
}