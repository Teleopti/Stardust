using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class AdherenceTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterType<FakeAdherencePercentageReadModelPersister>()
				.As<IAdherencePercentageReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeAdherenceDetailsReadModelPersister>()
				.As<IAdherenceDetailsReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeTeamOutOfAdherenceReadModelPersister>()
				.As<ITeamOutOfAdherenceReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeSiteOutOfAdherenceReadModelPersister>()
				.As<ISiteOutOfAdherenceReadModelPersister>()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<FakePerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
			builder.RegisterType<FakeLiteTransactionSyncronization>().As<ILiteTransactionSyncronization>().SingleInstance();
		}
	}

	public class FakeLiteTransactionSyncronization : ILiteTransactionSyncronization
	{
		public void OnSuccessfulTransaction(Action action)
		{
		}
	}

	public class FakePerformanceCounter : IPerformanceCounter
	{
		public bool IsEnabled { get; private set; }
		public int Limit { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string DataSource { get; set; }
		public void Count()
		{
		}

		public void ResetCount()
		{
		}
	}
}