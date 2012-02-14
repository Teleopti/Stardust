using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface ILayerConstructor<TLayerType>
    {
        TLayerType CreateLayer(IActivity activity, DateTimePeriod period);
    }
}