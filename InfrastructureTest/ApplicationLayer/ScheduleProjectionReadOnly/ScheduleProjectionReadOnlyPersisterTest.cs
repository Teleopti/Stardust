using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[UnitOfWorkWithLoginTest]
	public class ScheduleProjectionReadOnlyPersisterTest
	{
		public IScheduleProjectionReadOnlyPersister Persister;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldAddLayer()
		{
			var scenario = Guid.NewGuid();
			var person = Guid.NewGuid();

			Persister.AddProjectedLayer(
				"2016-04-29".Date(),
				scenario,
				person,
				new ProjectionChangedEventLayer
				{
					StartDateTime = "2016-04-29 8:00".Utc(),
					EndDateTime = "2016-04-29 17:00".Utc(),
				},
				"2016-04-29 12:00".Utc());

			Persister.ForPerson("2016-04-29".Date(), person, scenario).Should().Have.Count.EqualTo(1);
		}
	}
}