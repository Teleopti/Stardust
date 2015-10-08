using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class KpiRepositoryTest : RepositoryTest<IKeyPerformanceIndicator>
    {
        

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        { 
            //SkipRollback();
        }

        protected override IKeyPerformanceIndicator CreateAggregateWithCorrectBusinessUnit()
        {
            KeyPerformanceIndicator real = new KeyPerformanceIndicator();
            
            return real;
        }

		[Test]
		public void CannotModifyKpi()
		{
			var kpi = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(kpi);
			typeof(KeyPerformanceIndicator).GetField("_resourceKey", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(kpi,"changed");
			PersistAndRemoveFromUnitOfWork(kpi);

			Session.Get<KeyPerformanceIndicator>(kpi.Id.Value).ResourceKey
				.Should().Not.Be.EqualTo("changed");
		}

        protected override void VerifyAggregateGraphProperties(IKeyPerformanceIndicator loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual(" ", CreateAggregateWithCorrectBusinessUnit().Name);
        }

        protected override Repository<IKeyPerformanceIndicator> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new KpiRepository(currentUnitOfWork.Current());
        }
    }
}
