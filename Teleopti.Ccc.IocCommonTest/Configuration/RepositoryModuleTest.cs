using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class RepositoryModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
		}

		[Test]
		public void AllRepositoriesWithCorrectCtorAreWired()
		{
			using (var container = containerBuilder.Build())
			{
				var personRep = container.Resolve<IPersonRepository>();
				var skillRep = container.Resolve<ISkillRepository>();

				Assert.IsAssignableFrom<PersonRepository>(personRep);
				Assert.IsAssignableFrom<SkillRepository>(skillRep);
			}
		}

		[Test]
		public void PushMessageRepositoryShouldBeWired()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IPushMessageRepository>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveTheStatisticRepository()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IStatisticRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveTheAgentBadgeSettingRepository()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IAgentBadgeSettingsRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveEtlLogObjectRepository()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IEtlLogObjectRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveEtlJobStatusRepository()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IEtlJobStatusRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void RepositoriesWithIncorrectCtorAreNotWired()
		{
			var typeThatNoRepoAcceptAsArgument = GetType();
			var testContainerBuild = new ContainerBuilder();
			testContainerBuild.RegisterModule(new CommonModule { RepositoryConstructorType = typeThatNoRepoAcceptAsArgument });

			using (var container = testContainerBuild.Build())
			{
				container.IsRegistered(typeof(IPersonRepository))
					.Should().Be.False();
			}
		}
	}
}