using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PersonScheduleDayReadModelRepositoryTest : DatabaseTest
	{
		private PersonScheduleDayReadModelRepository _target;
 
		[Test]
		public void ShouldReturnReadModelsForPerson()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);	
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.ReadModelsOnPerson(dateOnly, dateOnly.AddDays(5), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId);

				var ret = _target.ReadModelsOnPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), personId);

				Assert.That(ret.Count, Is.EqualTo(1));
			}
		}
		
		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.IsInitialized(), Is.False);

				createAndSaveReadModel(personId, teamId);

				Assert.That(_target.IsInitialized(), Is.True);

				_target.ClearPeriodForPerson(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2)), personId);

				Assert.That(_target.IsInitialized(), Is.False);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForTeam()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId);
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = _target.ForTeam(new DateTimePeriod(new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 29, 12, 0, 0, DateTimeKind.Utc)), teamId);
				Assert.That(ret.Count, Is.EqualTo(1));
			}
		}

		private void createAndSaveReadModel(Guid personId, Guid teamId)
			{
				var model = new PersonScheduleDayReadModel
				            	{
				            		Date = new DateTime(2012, 8, 29),
				            		TeamId = teamId,
				            		PersonId = personId,
				            		ShiftStart = new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc),
				            		ShiftEnd = new DateTime(2012, 8, 29, 18, 0, 0, DateTimeKind.Utc),
				            		Shift = "",
				            	};

				_target.SaveReadModel(model);
			}
		}

	
}