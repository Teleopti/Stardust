using Autofac;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollBusBootStrapper : BusBootStrapper
	{
		public PayrollBusBootStrapper(IContainer container)
			: base(container)
		{
		}

		//protected override void OnEndStart()
		//{
		//	base.OnEndStart();

		//	Task.Run(() =>
		//	{
		//		var initializePayrollFormats = new InitializePayrollFormatsToDb(Container.Resolve<IPlugInLoader>(), Container.Resolve<DataSourceForTenantWrapper>().DataSource()());
		//		initializePayrollFormats.Initialize();
		//	});
		//}
	}
}