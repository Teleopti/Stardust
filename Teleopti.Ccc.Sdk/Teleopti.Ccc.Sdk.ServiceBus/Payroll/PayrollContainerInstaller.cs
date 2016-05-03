using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	//This must be in this project and can't be moved to domain
	//because som classes are dependent of a reference to Sdk
    public class PayrollContainerInstaller : Module
    {
	  protected override void Load(ContainerBuilder builder)
		{
			
			builder.RegisterType<PayrollExportHandler>().As<IHandleEvent<RunPayrollExportEvent>>().AsSelf().ApplyAspects();
			builder.RegisterType<PlugInLoader>().As<IPlugInLoader>().SingleInstance();
			builder.RegisterType<DataSourceForTenantWrapper>().SingleInstance();
			builder.RegisterType<SearchPath>().As<ISearchPath>().SingleInstance();
			builder.RegisterType<DomainAssemblyResolver>().As<IDomainAssemblyResolver>().SingleInstance();
			builder.RegisterType<AssemblyFileLoader>().As<IAssemblyFileLoader>().SingleInstance();
			builder.RegisterType<InitializePayrollFormatsToDb>().As<IInitializePayrollFormats>().SingleInstance();
			builder.RegisterType<ChannelCreator>().As<IChannelCreator>();
			builder.RegisterType<PayrollDataExtractor>().As<IPayrollDataExtractor>();
			builder.RegisterType<PersonBusAssembler>().As<IPersonBusAssembler>();
			builder.RegisterType<PayrollExportFeedback>().As<IServiceBusPayrollExportFeedback>();
			builder.RegisterType<PayrollPeopleLoader>().As<IPayrollPeopleLoader>();
			builder.RegisterType<emptyTenantPeopleLoader>().As<ITenantPeopleLoader>();
		}

		private class emptyTenantPeopleLoader : ITenantPeopleLoader
		{
			public void FillDtosWithLogonInfo(IList<PersonDto> personDtos)
			{

			}
		}
    }
}