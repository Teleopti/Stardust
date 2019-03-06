using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection2;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class RepositoryModuleTest
	{
		private ContainerBuilder builder;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
		}

		[Test]
		public void AllRepositoriesWithCorrectCtorAreWired()
		{
			using (var container = builder.Build())
			{
				var personRep = container.Resolve<IPersonRepository>();
				var skillRep = container.Resolve<ISkillRepository>();
				var skillTypeRep = container.Resolve<ISkillTypeRepository>();

				Assert.IsAssignableFrom<PersonRepository>(personRep);
				Assert.IsAssignableFrom<SkillRepository>(skillRep);
				Assert.IsAssignableFrom<SkillTypeRepository>(skillTypeRep);
			}
		}
		
		[Test]
		public void AllRepositoriesCanResolve()
		{
			var resolveAs = typeof(PersonRepository).Assembly
				.GetExportedTypes()
				.Where(RepositoryDetector.RegisteredAsRepository)
				.Where(x => !cantResolveProperly(x))
				.SelectMany(x => x.GetInterfaces());

			using (var container = builder.Build())
			{
				resolveAs.ForEach(x =>
				{
					container.Resolve(x)
						.Should().Not.Be.Null();
				});
			}

			bool cantResolveProperly(Type type)
			{
				if (type == typeof(SkillForecastJobStartTimeRepository))
					return true;
				if (type == typeof(SkillForecastReadModelRepository))
					return true;
				if (type.GetConstructors().Length == 1 && type.GetConstructor(new[] {typeof(IUnitOfWork)}) != null)
					return true;
				if (type.GetConstructors().Length == 1 && type.GetConstructor(new[] {typeof(IStatelessUnitOfWork)}) != null)
					return true;
				return false;
			}
		}

		[Test]
		public void PushMessageRepositoryShouldBeWired()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IPushMessageRepository>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveTheStatisticRepository()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IStatisticRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveEtlLogObjectRepository()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IEtlLogObjectRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveEtlJobStatusRepository()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IEtlJobStatusRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveScheduleRepository()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IScheduleStorage>().Should().Not.Be.Null();
			}
		}
	}
}