using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[Category("LongRunning")]
	public class BugShiftExchangeInvalidUpdate : DatabaseTest
	{
		private IPersonRequestRepository personRequestRepository;

		protected override void SetupForRepositoryTest()
		{
			personRequestRepository = new PersonRequestRepository(SetupFixtureForAssembly.DataSource.Application);
			
			//make sure setup data is persisted
			CleanUpAfterTest();
			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldNotUpdateShiftExchangeOfferOnRead()
		{
			Guid id;
			//save
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var pr = createShiftExchangeOffer();
				personRequestRepository.Add(pr);
				id = pr.Id.Value;
				uow.PersistAll();
			}
			//load, change nothing and commit. 
			int version;
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var pr = personRequestRepository.Get(id);
				version = ((IVersioned)pr).Version.Value;
				pr.Request.Root(); //make sure shift trade is loaded by calling whatever on the IRequest
				uow.PersistAll();
			}
			//Version number should not be increased
			using (SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				((IVersioned) personRequestRepository.Get(id)).Version.Value
					.Should().Be.EqualTo(version);
			}

			cleanup(id);
		}

		private void cleanup(Guid id)
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				personRequestRepository.Remove(personRequestRepository.Find(id));
				uow.PersistAll();
			}
		}


		private IPersonRequest createShiftExchangeOffer()
		{
			IPersonRequest request = new PersonRequest(SetupFixtureForAssembly.loggedOnPerson);

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
			new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create(new DateOnly(2007, 1, 1), request.Person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.SetId(new Guid("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift(EditableShiftFactory.CreateEditorShift(activity,
				period.ChangeEndTime(TimeSpan.FromHours(3)),
				ShiftCategoryFactory.CreateShiftCategory("Early")));

			var startDateTime  = new DateTime (2007, 01, 01, 12, 0, 0).ToUniversalTime();
			var endDateTime = new DateTime (2007, 01, 01, 22, 0, 0).ToUniversalTime();

			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift,
				new DateTimePeriod(startDateTime, endDateTime));
			
			var offer = new ShiftExchangeOffer(currentShift,
				new ShiftExchangeCriteria(new DateOnly(2013, 12, 31), dayFilterCriteria), ShiftExchangeOfferStatus.Pending);
			
			request.Request = offer;
			return request;
		}
	}
}