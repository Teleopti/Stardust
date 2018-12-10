using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface ILayerConstructor<TLayerType>
    {
        TLayerType CreateLayer(IActivity activity, DateTimePeriod period);
    }
}