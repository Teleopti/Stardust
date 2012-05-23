
using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class DenormalizePersonFinderConsumer : ConsumerOf<DenormalizePersonFinder >
	{
    	private readonly IUpdatePersonFinderReadModel  _updatePersonFinderReadModel;

        public DenormalizePersonFinderConsumer(IUpdatePersonFinderReadModel updatePersonFinderReadModel)
		{
            _updatePersonFinderReadModel = updatePersonFinderReadModel;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(DenormalizePersonFinder  message)
		{
            _updatePersonFinderReadModel.Execute(message.IsPerson , message.Ids);

		}
	}
}
