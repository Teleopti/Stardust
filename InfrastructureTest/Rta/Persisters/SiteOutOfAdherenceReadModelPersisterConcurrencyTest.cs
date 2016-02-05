using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class SiteOutOfAdherenceReadModelPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public TheService Service { get; set; }

		[Test]
		public void ShouldNotRetryWhenMultipleWorkersUpdatesSameModel()
		{
			var siteId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			simulator.ProcessAsync(() => Service.GetAndIncrementCount(siteId));
			simulator.WaitForAll();

			10.Times(() => simulator.ProcessAsync(() => Service.GetAndIncrementCount(siteId)));
			10.Times(() => simulator.ProcessAsync(() => Service.GetAllAndIncrementCount(siteId)));
			simulator.WaitForAll();

			simulator.RetryCount.Should().Be(0);
			Service.Get(siteId).Count.Should().Be(21);
		}

		[Test]
		public void ShouldCalculateCorrectlyWhenMultipleWorkersAddTheSameModel()
		{
			var siteId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			10.Times(() => simulator.ProcessAsync(() => Service.GetAndIncrementCount(siteId)));
			10.Times(() => simulator.ProcessAsync(() => Service.GetAllAndIncrementCount(siteId)));
			simulator.WaitForAll();

			Service.Get(siteId).Count.Should().Be(20);
		}

		public class TheService
		{
			private readonly ISiteOutOfAdherenceReadModelPersister _persister;

			public TheService(ISiteOutOfAdherenceReadModelPersister persister)
			{
				_persister = persister;
			}

			[ReadModelUnitOfWork]
			public virtual void GetAndIncrementCount(Guid siteId)
			{
				incrementCount(_persister.Get(siteId), siteId);
			}

			[ReadModelUnitOfWork]
			public virtual void GetAllAndIncrementCount(Guid siteId)
			{
				incrementCount(_persister.GetAll().SingleOrDefault(), siteId);
			}

			private void incrementCount(SiteOutOfAdherenceReadModel model, Guid siteId)
			{
				if (model == null)
					model = new SiteOutOfAdherenceReadModel
					{
						SiteId = siteId
					};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.Count++;
				_persister.Persist(model);
			}

			[ReadModelUnitOfWork]
			public virtual SiteOutOfAdherenceReadModel Get(Guid siteId)
			{
				return _persister.Get(siteId);
			}
		}
		
	}
}