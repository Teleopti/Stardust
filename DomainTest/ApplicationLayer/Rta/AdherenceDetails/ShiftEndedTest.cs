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
	public class ShiftEndedTest : IRegisterInContainer
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherenceDetailsReadModelUpdater>().AsSelf();
		}

		[Test]
		public void ShouldMarkLastActivityEndedWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Name = "Phone",
				InAdherence = false
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId, 
				ShiftStartTime = "2014-11-17 8:00".Utc()
			});

			Persister.Model.HasShiftEnded.Should().Be(true);
		}
	}
}