using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class DatabaseReaderTest
	{
		public IDatabaseReader Reader;
		public IAgentStateReadModelPersister Writer;
		public MutableNow Now;

		[Test]
		public void ShouldReadBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new ScheduleProjectionReadOnlyPersister(new ThisUnitOfWork(uow));
				repository.AddActivity(new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2014-11-07".Date(),
					ScenarioId = Guid.NewGuid(),
					PersonId = personId,
					StartDateTime = "2014-11-07 10:00".Utc(),
					EndDateTime = "2014-11-07 10:00".Utc()
				});
				uow.PersistAll();
			}
			
			var result = Reader.GetCurrentSchedule(personId);

			result.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}
	}
}