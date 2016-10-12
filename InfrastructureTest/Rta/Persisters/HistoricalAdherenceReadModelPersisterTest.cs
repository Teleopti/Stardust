using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	[Ignore("WIP")]
	public class HistoricalAdherenceReadModelPersisterTest
	{
		public IHistoricalAdherenceReadModelPersister Target;
		public IHistoricalAdherenceReadModelReader Reader;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldPersistReadModel()
		{
			var personId = Guid.NewGuid();
			var state = new HistoricalAdherenceReadModelForTest
			{
				PersonId = personId,
				Date = "2016-10-10".Date()
			};

			Reader.Read(personId, new DateOnly()).Date
				.Should().Be.Null();
		}

		[Test]
		public void ShouldPersistOutOfAdherences()
		{
			var state = new HistoricalAdherenceReadModelForTest
			{
				OutOfAdherences = new[]
				{
					new HistoricalOutOfAdherenceReadModel()
					{
						StartTime = "2016-10-11 08:00".Utc(),
						EndTime = "2016-10-11 08:10".Utc()
					}
				}
			};

			var outOfAdherence = Reader.Read(state.PersonId, new DateOnly())
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-10-11 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-10-11 08:10".Utc());
		}
		
	}
}