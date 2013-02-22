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
			Assert.That(_target.ReadModelsOnPerson(dateOnly, dateOnly.AddDays(5), personId), Is.Not.Null);
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForPerson()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			createAndSaveReadModel(personId,teamId);
			
			var ret = _target.ReadModelsOnPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), personId);
			
			Assert.That(ret.Count, Is.EqualTo(1));
		}
		
		[Test]
		public void ShouldIndicateIfInitializedOrNot()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);

			Assert.That(_target.IsInitialized(), Is.False);

			createAndSaveReadModel(personId, teamId);

			Assert.That(_target.IsInitialized(), Is.True);

			_target.ClearPeriodForPerson(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2)), personId);
			
			Assert.That(_target.IsInitialized(), Is.False);
		}

		[Test]
		public void ShouldSaveAndLoadReadModelForTeam()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			createAndSaveReadModel(personId, teamId);

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

		[Test]
		public void ShouldNotCrashIfShiftIsBiggerThanFourThousand()
		{
			_target = new PersonScheduleDayReadModelRepository(UnitOfWorkFactory.Current);
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			const string shift = @"{\'FirstName\':\'????????? ?????\',\'LastName\':\'7004202\',\'EmploymentNumber\':\'\',\'Id\':\'4b9853d6-4073-48d4-a9b0-9e3f0101ff55\',\'Date\':\'2012-01-12T00:00:00\',\'WorkTimeMinutes\':664,\'ContractTimeMinutes\':664,\'Projection\':[{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T09:45:00Z\',\'End\':\'2012-01-12T10:52:00Z\',\'Minutes\':67,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T10:52:00Z\',\'End\':\'2012-01-12T10:55:00Z\',\'Minutes\':3,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T10:55:00Z\',\'End\':\'2012-01-12T11:00:00Z\',\'Minutes\':5,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T11:00:00Z\',\'End\':\'2012-01-12T11:15:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T11:15:00Z\',\'End\':\'2012-01-12T11:34:00Z\',\'Minutes\':19,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T11:34:
00Z\',\'End\':\'2012-01-12T11:43:00Z\',\'Minutes\':9,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T11:43:00Z\',\'End\':\'2012-01-12T12:15:00Z\',\'Minutes\':32,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T12:15:00Z\',\'End\':\'2012-01-12T12:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T12:30:00Z\',\'End\':\'2012-01-12T13:45:00Z\',\'Minutes\':75,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T13:45:00Z\',\'End\':\'2012-01-12T14:00:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T14:00:00Z\',\'End\':\'2012-01-12T15:01:00Z\',\'Minutes\':61,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T15:01:00Z\',\'End\':\'2012-01-12T15:05:00Z\',\'Minutes\':4,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T15:05:00Z\',\'En
d\':\'2012-01-12T15:14:00Z\',\'Minutes\':9,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T15:14:00Z\',\'End\':\'2012-01-12T15:19:00Z\',\'Minutes\':5,\'Title\':\'????????? ??????\'},{\'Color\':\'#FFFF00\',\'Start\':\'2012-01-12T15:19:00Z\',\'End\':\'2012-01-12T16:15:00Z\',\'Minutes\':56,\'Title\':\'???? / Lunch\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T16:15:00Z\',\'End\':\'2012-01-12T16:41:00Z\',\'Minutes\':26,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T16:41:00Z\',\'End\':\'2012-01-12T16:45:00Z\',\'Minutes\':4,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T16:45:00Z\',\'End\':\'2012-01-12T17:15:00Z\',\'Minutes\':30,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T17:15:00Z\',\'End\':\'2012-01-12T17:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T17:30:00Z\',\'End\':\'2012-01-12T17:45:00Z\',\'Minutes\':15
,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T17:45:00Z\',\'End\':\'2012-01-12T17:52:00Z\',\'Minutes\':7,\'Title\':\'????????? ??????\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T17:52:00Z\',\'End\':\'2012-01-12T18:45:00Z\',\'Minutes\':53,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T18:45:00Z\',\'End\':\'2012-01-12T19:00:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T19:00:00Z\',\'End\':\'2012-01-12T20:15:00Z\',\'Minutes\':75,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#FF0000\',\'Start\':\'2012-01-12T20:15:00Z\',\'End\':\'2012-01-12T20:30:00Z\',\'Minutes\':15,\'Title\':\'??????? / Personal\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T20:30:00Z\',\'End\':\'2012-01-12T21:16:00Z\',\'Minutes\':46,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T21:16:00Z\',\'End\':\'2012-01-12T21:20:00Z\',\'Minutes\':4,\'Title\':\'????????? ??????
??? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T21:20:00Z\',\'End\':\'2012-01-12T21:35:00Z\',\'Minutes\':15,\'Title\':\'??????? / ????? ???????\'},{\'Color\':\'#000000\',\'Start\':\'2012-01-12T21:35:00Z\',\'End\':\'2012-01-12T21:42:00Z\',\'Minutes\':7,\'Title\':\'????????? ????????? (??????) / Techn pause\'},{\'Color\':\'#00FF00\',\'Start\':\'2012-01-12T21:42:00Z\',\'End\':\'2012-01-12T21:45:00Z\',\'Minutes\':3,\'Title\':\'??????? / ????? ???????\'}]}";

			var model = new PersonScheduleDayReadModel
			{
				Date = new DateTime(2012, 8, 29),
				TeamId = teamId,
				PersonId = personId,
				ShiftStart = new DateTime(2012, 8, 29, 10, 0, 0, DateTimeKind.Utc),
				ShiftEnd = new DateTime(2012, 8, 29, 18, 0, 0, DateTimeKind.Utc),
				Shift = shift,
			};
			_target.SaveReadModel(model);
		}
	}
}

