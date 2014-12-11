using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

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

				Assert.IsAssignableFrom<PersonRepository>(personRep);
				Assert.IsAssignableFrom<SkillRepository>(skillRep);
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
		public void ShouldResolveTheAgentBadgeSettingRepository()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IAgentBadgeSettingsRepository>().Should().Not.Be.Null();
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
		public void RepositoriesWithIncorrectCtorAreNotWired()
		{
			var typeThatNoRepoAcceptAsArgument = GetType();
			var config = new IocConfiguration(new IocArgs { DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest() }, new TrueToggleManager());
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(config) { RepositoryConstructorType = typeThatNoRepoAcceptAsArgument });

			using (var container = builder.Build())
			{
				container.IsRegistered(typeof(IPersonRepository))
					.Should().Be.False();
			}
		}
	}
}