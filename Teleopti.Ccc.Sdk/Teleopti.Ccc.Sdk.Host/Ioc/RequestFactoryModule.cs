using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;

namespace Teleopti.Ccc.Sdk.WcfHost.Ioc
{
	public class RequestFactoryModule : Module
	{
		private IIocConfiguration _configuration;

		public RequestFactoryModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RepositoryFactory>()
				.As<IRepositoryFactory>()
				.SingleInstance();
			builder.RegisterType<PersonRequestFactory>()
				.As<IPersonRequestFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PersistPersonRequest>()
				.As<IPersistPersonRequest>().InstancePerLifetimeScope();

			builder.RegisterType<SdkPersonRequestAuthorizationCheck>().As<IPersonRequestCheckAuthorization>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftTradeRequestStatusChecker>().As<IBatchShiftTradeRequestStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>().InstancePerDependency();
			builder.RegisterType<SwapService>().As<ISwapService>().InstancePerDependency();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>().InstancePerDependency();
			if (_configuration.Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_43447))
			{
				builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceRepository>().SingleInstance();
				builder.RegisterType<ScheduleDayDifferenceSaveTemporary>().As<IScheduleDayDifferenceSaveTemporary>().SingleInstance();
			}
			if (_configuration.Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step2_44271))
			{
				builder.RegisterType<ScheduleDayDifferenceSaveTemporaryEmpty>().As<IScheduleDayDifferenceSaveTemporary>().SingleInstance();
				builder.RegisterType<ScheduleDayDifferenceSaver>().As<IScheduleDayDifferenceSaver>().SingleInstance();
			}

		}
	}
}
