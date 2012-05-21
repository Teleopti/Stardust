
namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IUpdateGroupingReadModel
	{
		void Execute(int type, string ids);
	}
}