using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using log4net;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.WcfHost
{
	public class SendDenormalizeNotificationToBus : ISendDenormalizeNotification
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(SendDenormalizeNotificationToBus));
		private readonly Func<IHandleCommand<DenormalizeNotificationCommandDto>> _serviceBusReference;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",Justification = "This is done to be able to get the service bus handle properly from the container.")]
		public SendDenormalizeNotificationToBus(Func<IHandleCommand<DenormalizeNotificationCommandDto>> serviceBusReference)
		{
			_serviceBusReference = serviceBusReference;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Notify()
		{
			try
			{
				var handler = _serviceBusReference.Invoke();
				handler.Handle(new DenormalizeNotificationCommandDto());
			}
			catch (FaultException exception)
			{
				Logger.Error("An error occurred when trying to send denormalizer message.",exception);
			}
		}
	}
}