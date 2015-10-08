using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RepositoryModule : Module
	{
		public Type RepositoryConstructorType { get; set; }

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
				else if (type.GetConstructor(new[] {RepositoryConstructorType}) != null)
				{
					builder.RegisterType(type)
						.UsingConstructor(RepositoryConstructorType)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
			}

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
					if (parameters[0].ParameterType == RepositoryConstructorType)
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