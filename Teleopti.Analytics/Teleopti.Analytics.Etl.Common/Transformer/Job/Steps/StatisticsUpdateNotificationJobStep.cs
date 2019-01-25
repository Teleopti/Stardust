using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Messaging.Client;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StatisticsUpdateNotificationJobStep : JobStepBase
	{
		public StatisticsUpdateNotificationJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "Statistics Update Notification";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Send to MessageBroker that new queue stats is loaded
			var messageSender = _jobParameters.Helper.MessageSender;

			var identity = (ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity;

			var notification = NotificationFactory.CreateNotification(
				DateTime.Now.Date,
				DateTime.Now.Date,
				Guid.NewGuid(),
				Guid.Empty,
				typeof (IStatisticTask),
				identity.DataSource.DataSourceName,
				Guid.Empty);

			messageSender.Send(notification);

			return 0;
		}
	}
}