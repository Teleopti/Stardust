using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	[Serializable]
	public class SdkServiceFactory : ISdkServiceFactory
	{
		private readonly IChannelCreator _channelCreator;

		public SdkServiceFactory(IChannelCreator channelCreator)
		{
			_channelCreator = channelCreator;
		}

		public ITeleoptiSchedulingService CreateTeleoptiSchedulingService()
		{
			return _channelCreator.CreateChannel<ITeleoptiSchedulingService>();
		}

		public IPayrollExportFeedback CreatePayrollExportFeedback(InterAppDomainArguments interAppDomainArguments)
		{
			return new PayrollExportFeedbackEx(interAppDomainArguments);
		}

		public ITeleoptiOrganizationService CreateTeleoptiOrganizationService()
		{
			return _channelCreator.CreateChannel<ITeleoptiOrganizationService>();
		}
	}
}