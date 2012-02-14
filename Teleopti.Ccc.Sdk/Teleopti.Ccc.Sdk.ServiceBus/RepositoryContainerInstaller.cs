using System;
using Autofac;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class RepositoryContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(PersonRepository).Assembly)
				.Where(t => isRepository(t) && hasCorrectCtor(t))
				.AsImplementedInterfaces()
				.InstancePerDependency();
		}

		private static bool hasCorrectCtor(Type repositoryType)
		{
			foreach (var constructorInfo in repositoryType.GetConstructors())
			{
				var parameters = constructorInfo.GetParameters();
				if (parameters.Length == 1)
				{
					if (parameters[0].ParameterType.Equals(typeof(IUnitOfWorkFactory)))
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
