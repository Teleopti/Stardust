namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IInitializePayrollFormats
	{
		void Initialize();
		void RefreshOneTenant(string tenantName);

	}
}