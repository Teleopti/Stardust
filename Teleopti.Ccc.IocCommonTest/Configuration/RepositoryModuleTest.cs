using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
			containerBuilder.RegisterModule(new RepositoryModule());
			containerBuilder.RegisterModule(new UnitOfWorkModule());
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
		public void ShouldResolveTheStatisticRepository()
		{
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IStatisticRepository>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void RepositoriesWithIncorrectCtorAreNotWired()
		{
            /*
             * RK
             * If this test fails because IWorkShiftRuleSetRepository accepts an iunitofworkfactory,
             * please change to another rep.
             * There is no reason why I test IWorkShiftRuleSetRepository below, just want a rep
             * where "new uow handling" not yet supported
             */

            using (var container = containerBuilder.Build())
			{
				Assert.IsFalse(container.IsRegistered(typeof (IWorkShiftRuleSetRepository)));
				Assert.IsTrue(container.IsRegistered(typeof(IPersonRepository)));
			}
		}
	}
}