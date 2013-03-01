using System;
using System.Linq;
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
		public void ShouldReturnReadModelsForPersonForDates()
		{
			_target = new PersonScheduleDayReadModelRepository(CurrentUnitOfWork.Make());	
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.ForPerson(dateOnly, dateOnly.AddDays(5), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldReturnReadModelForPersonDay()
		{
			_target = new PersonScheduleDayReadModelRepository(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, Guid.NewGuid(), new DateTime(2012, 8, 28));

				Assert.That(_target.ForPerson(new DateOnly(2012, 8, 28), personId), Is.Not.Null);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			_target = new PersonScheduleDayReadModelRepository(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));

				var ret = _target.ForPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), personId);

				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			_target = new PersonScheduleDayReadModelRepository(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.That(_target.IsInitialized(), Is.False);

				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));

				Assert.That(_target.IsInitialized(), Is.True);

				_target.ClearPeriodForPerson(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2)), personId);

				Assert.That(_target.IsInitialized(), Is.False);
			}
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForTeam()
		{
			_target = new PersonScheduleDayReadModelRepository(CurrentUnitOfWork.Make());
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndSaveReadModel(personId, teamId, new DateTime(2012, 8, 29));
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var ret = _target.ForTeam(new DateTimePeriod(new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 29, 12, 0, 0, DateTimeKind.Utc)), teamId);
				Assert.That(ret.Count(), Is.EqualTo(1));
			}
		}

		private void createAndSaveReadModel(Guid personId, Guid teamId, DateTime date)
			{
				var model = new PersonScheduleDayReadModel
				            	{
				            		Date = date,
				            		TeamId = teamId,
				            		PersonId = personId,
				            		ShiftStart = date.AddHours(10),
									ShiftEnd = date.AddHours(18),
				            		Shift = "",
				            	};

				_target.SaveReadModel(model);
			}
		}

	
}