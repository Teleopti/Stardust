using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[RtaDatabaseTest]
	public class DatabaseReaderTest
	{
		public IDatabaseReader Reader;
		public IDatabaseWriter Writer;
		public MutableNow Now;

		[Test]
		public void ShouldReadBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var layer = new ProjectionChangedEventLayer
				{
					StartDateTime = "2014-11-07 10:00".Utc(),
					EndDateTime = "2014-11-07 10:00".Utc()
				};
				var repository = new ScheduleProjectionReadOnlyRepository(new ThisUnitOfWork(uow));
				repository.AddProjectedLayer(new DateOnly("2014-11-07".Utc()), Guid.NewGuid(), personId, layer);
				uow.PersistAll();
			}
			
			var result = Reader.GetCurrentSchedule(personId);

			result.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}
	}
}