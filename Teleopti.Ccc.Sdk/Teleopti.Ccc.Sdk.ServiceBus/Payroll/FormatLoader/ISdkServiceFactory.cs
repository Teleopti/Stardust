using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	public interface ISdkServiceFactory
	{
		ITeleoptiSchedulingService CreateTeleoptiSchedulingService();
		ITeleoptiOrganizationService CreateTeleoptiOrganizationService();
		IPayrollExportFeedback CreatePayrollExportFeedback(InterAppDomainArguments intrAppDomainArguments);
	}
}