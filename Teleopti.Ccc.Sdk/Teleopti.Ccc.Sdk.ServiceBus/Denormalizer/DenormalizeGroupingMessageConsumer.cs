using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class DenormalizeGroupingMessageConsumer : ConsumerOf<DenormalizeGroupingMessage>
	{
    	private readonly IUpdateGroupingReadModel  _updateGroupingReadModel;

		public DenormalizeGroupingMessageConsumer( IUpdateGroupingReadModel updateGroupingReadModel)
		{
			_updateGroupingReadModel = updateGroupingReadModel;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeGroupingMessage message)
		{
			_updateGroupingReadModel.Execute(message.GroupingType, message.Ids);

		}
	}
}