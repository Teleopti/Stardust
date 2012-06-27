namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public interface IUpdatePersonFinderReadModel
    {
        void Execute(bool isPerson, string ids);
    }
}
