using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RepositoryModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(PersonRepository).Assembly)
				.Where(t => isRepository(t) && hasCorrectCtor(t))
				.AsImplementedInterfaces()
				.InstancePerDependency();
			builder.Register(c => StatisticRepositoryFactory.Create()).As<IStatisticRepository>();
		}

		private static bool hasCorrectCtor(Type repositoryType)
		{
			foreach (var constructorInfo in repositoryType.GetConstructors())
			{
				var parameters = constructorInfo.GetParameters();
				if (parameters.Count() == 1)
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