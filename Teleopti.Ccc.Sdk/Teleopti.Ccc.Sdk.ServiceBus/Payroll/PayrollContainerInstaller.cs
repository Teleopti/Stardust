using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	//This must be in this project and can't be moved to domain
	//because som classes are dependent of a reference to Sdk
    public class PayrollContainerInstaller : Module
    {
		private readonly IocConfiguration _configuration;

		public PayrollContainerInstaller(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PlugInLoader>().As<IPlugInLoader>().SingleInstance();
			builder.RegisterType<DataSourceForTenantWrapper>().SingleInstance();
			builder.RegisterType<SearchPath>().As<ISearchPath>().SingleInstance();
			
			//Need to check if that is going to work for the container or not.
			if (_configuration.IsToggleEnabled(Toggles.Wfm_Payroll_SupportMultiDllPayrolls_75959))
			{
				builder.RegisterType<PayrollExportHandlerNew>().As<IHandleEvent<RunPayrollExportEvent>>().ApplyAspects();
				builder.RegisterType<InitializePayrollFormatsUsingAppDomain>().As<IInitializePayrollFormats>().SingleInstance();
				builder.RegisterType<DomainAssemblyResolverNew>().As<IDomainAssemblyResolver>().SingleInstance();
				builder.RegisterType<AssemblyFileLoaderTenant>().As<IAssemblyFileLoader>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PayrollExportHandler>().As<IHandleEvent<RunPayrollExportEvent>>().ApplyAspects();
				builder.RegisterType<InitializePayrollFormatsToDb>().As<IInitializePayrollFormats>().SingleInstance();
				builder.RegisterType<DomainAssemblyResolverOld>().As<IDomainAssemblyResolver>().SingleInstance();
				builder.RegisterType<AssemblyFileLoader>().As<IAssemblyFileLoader>().SingleInstance();
			}

			builder.RegisterType<ChannelCreator>().As<IChannelCreator>();
			builder.RegisterType<PayrollDataExtractor>().As<IPayrollDataExtractor>();
			builder.RegisterType<PersonBusAssembler>().As<IPersonBusAssembler>();
			builder.RegisterType<PayrollExportFeedback>().As<IServiceBusPayrollExportFeedback>();
			builder.RegisterType<PayrollPeopleLoader>().As<IPayrollPeopleLoader>();
			builder.RegisterType<emptyTenantPeopleLoader>().As<ITenantPeopleLoader>();
			builder.RegisterType<SdkServiceFactory>().As<ISdkServiceFactory>();
		}

		private class emptyTenantPeopleLoader : ITenantPeopleLoader
		{
			public void FillDtosWithLogonInfo(IList<PersonDto> personDtos)
			{

			}
		}
    }
}