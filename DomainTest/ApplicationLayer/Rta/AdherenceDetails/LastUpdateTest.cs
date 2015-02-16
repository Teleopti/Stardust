using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class LastUpdateTest : IRegisterInContainer
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherenceDetailsReadModelUpdater>().AsSelf();
		}

		[Test]
		public void ShouldPersistLastAdherenceTrue()
		{
			Target.Handle(new PersonStateChangedEvent
			{
				InAdherence = true
			});

			Persister.Model.LastAdherence.Should().Be.True();
		}

		[Test]
		public void ShouldPersistLastAdherenceFalse()
		{
			Target.Handle(new PersonActivityStartEvent()
			{
				InAdherence = false
			});

			Persister.Model.LastAdherence.Should().Be.False();
		}

		[Test]
		public void ShouldPersistLastUpdateTime()
		{
			Target.Handle(new PersonStateChangedEvent
			{
				Timestamp = "2014-11-17 7:00".Utc(),
			});

			Persister.Model.LastUpdate.Should().Be("2014-11-17 7:00".Utc());
		}
	}
}