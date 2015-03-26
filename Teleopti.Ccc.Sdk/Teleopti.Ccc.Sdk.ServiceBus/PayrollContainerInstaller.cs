using Autofac;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class PayrollContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PlugInLoader>().As<IPlugInLoader>().SingleInstance();
			builder.RegisterType<SearchPath>().As<ISearchPath>().SingleInstance();
			builder.RegisterType<DomainAssemblyResolver>().As<IDomainAssemblyResolver>().SingleInstance();
			builder.RegisterType<AssemblyFileLoader>().As<IAssemblyFileLoader>().SingleInstance();
			builder.RegisterType<InitializePayrollFormatsToDb>().As<IInitializePayrollFormats>().SingleInstance();
			builder.RegisterType<ChannelCreator>().As<IChannelCreator>();
			builder.RegisterType<PayrollDataExtractor>().As<IPayrollDataExtractor>();
			builder.RegisterType<PersonBusAssembler>().As<IPersonBusAssembler>();
			builder.RegisterType<PayrollExportFeedback>().As<IServiceBusPayrollExportFeedback>();
			builder.RegisterType<PayrollPeopleLoader>().As<IPayrollPeopleLoader>();
		}
    }
}