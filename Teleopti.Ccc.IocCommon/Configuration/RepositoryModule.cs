using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RepositoryModule : Module
	{
		public RepositoryModule()
		{
			ConstructorTypeToUse = typeof (ICurrentUnitOfWork);
		}

		//hack to get desktop app use old behavior
		public Type ConstructorTypeToUse { get; set; }

		protected override void Load(ContainerBuilder builder)
		{
			foreach (var type in typeof(PersonRepository).Assembly.GetTypes().Where(t => isRepository(t) && hasCorrectCtor(t)))
			{
				if (type.GetConstructor(new[]{ConstructorTypeToUse}) != null)
				{
					builder.RegisterType(type)
								 .UsingConstructor(ConstructorTypeToUse)
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

			builder.Register(c => StatisticRepositoryFactory.Create())
				.As<IStatisticRepository>();


			builder.RegisterType<DefaultScenarioFromRepository>()
			       .As<ICurrentScenario>()
			       .InstancePerDependency();
		}

		private bool hasCorrectCtor(Type repositoryType)
		{
			foreach (var constructorInfo in repositoryType.GetConstructors())
			{
				var parameters = constructorInfo.GetParameters();
				if (parameters.Count() == 1)
				{
					if (parameters[0].ParameterType == ConstructorTypeToUse)
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