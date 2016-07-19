using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveBatchRequestsCommandHandler : IHandleCommand<ApproveBatchRequestsCommand>
	{
		#region Implementation of IHandleCommand<ApproveBatchRequestsCommand>
		private readonly IEventPublisher _publisher;

		public ApproveBatchRequestsCommandHandler(IEventPublisher publisher)
		{
			_publisher = publisher;
		}
		public void Handle(ApproveBatchRequestsCommand command)
		{
			var @event = new ApproveRequestsWithValidatorsEvent
			{
				PersonRequestIdList = command.PersonRequestIdList,
				TrackedCommandInfo = command.TrackedCommandInfo,
				Validator = command.Validator
			};
			_publisher.Publish(@event);
		}
		#endregion
	}
}