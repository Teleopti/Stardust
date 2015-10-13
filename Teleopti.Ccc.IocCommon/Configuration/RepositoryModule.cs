using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RepositoryModule : Module
	{
		private readonly Type repositoryConstructorType = typeof (ICurrentUnitOfWork);

		protected override void Load(ContainerBuilder builder)
		{
			foreach (var type in typeof(PersonRepository).Assembly.GetTypes().Where(t => isRepository(t) && hasCorrectCtor(t)))
			{
				if (type.GetConstructors().Length == 1)
				{
					builder.RegisterType(type)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
				else if (type.GetConstructor(new[] { repositoryConstructorType }) != null)
				{
					builder.RegisterType(type)
						.UsingConstructor(repositoryConstructorType)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
			}

			//make this one an explicit override now -> later make code above generic with repos with multiple ctors with multiple args
			builder.RegisterType<ScheduleRepository>()
				.UsingConstructor(typeof(ICurrentUnitOfWork), typeof(IRepositoryFactory))
				.AsImplementedInterfaces()
				.SingleInstance();
			//

			builder.RegisterType<PushMessagePersister>()
				.As<IPushMessagePersister>()
				.SingleInstance();
			builder.RegisterType<CreatePushMessageDialoguesService>()
				.As<ICreatePushMessageDialoguesService>()
				.SingleInstance();
			builder.RegisterType<EtlJobStatusRepository>()
				.As<IEtlJobStatusRepository>()
				.SingleInstance();
			builder.RegisterType<EtlLogObjectRepository>()
				.As<IEtlLogObjectRepository>()
				.SingleInstance();

			builder.Register(c => StatisticRepositoryFactory.Create())
				.As<IStatisticRepository>();

			builder.RegisterType<DefaultScenarioFromRepository>()
			       .As<ICurrentScenario>()
			       .InstancePerDependency();

			builder.RegisterType<LoadUserUnauthorized>()
				.As<ILoadUserUnauthorized>()
				.SingleInstance();
		}

		private bool hasCorrectCtor(Type repositoryType)
		{
			var constructors = repositoryType.GetConstructors();
			if (constructors.Length == 1)
				return true;
			foreach (var constructorInfo in constructors)
			{
				var parameters = constructorInfo.GetParameters();
				if (parameters.Count() == 1)
				{
					if (parameters[0].ParameterType == repositoryConstructorType)
						return true;
				}
			}
			return false;
		}

		private static bool isRepository(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("Repository", StringComparison.Ordinal);
		}
	}
}