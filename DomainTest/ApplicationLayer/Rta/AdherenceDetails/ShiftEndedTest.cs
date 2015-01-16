using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class ShiftEndedTest
	{
		[Test]
		public void ShouldMarkLastActivityEndedWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var liteUnitOfWorkSync = MockRepository.GenerateMock<ILiteTransactionSyncronization>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var target = new AdherenceDetailsReadModelUpdater(persister, liteUnitOfWorkSync, performanceCounter);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftStartTime = "2014-11-17 8:00".Utc() });

			persister.Model.HasShiftEnded.Should().Be(true);
		}
	}
}