﻿using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category ("LongRunning")]
	internal class SeatPlanRepositoryTest : RepositoryTest<ISeatPlan>
	{

		protected override ISeatPlan CreateAggregateWithCorrectBusinessUnit()
		{

			var seatPlan = new SeatPlan()
			{
				Date = new DateOnly(2015,03,02),
				Status = SeatPlanStatus.InError
			};

			return seatPlan;
			
		}

		protected override void VerifyAggregateGraphProperties (ISeatPlan loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.Id);
			Assert.IsNotNull(loadedAggregateFromDatabase.Date);
			Assert.IsNotNull(loadedAggregateFromDatabase.Status);
			Assert.AreEqual(SeatPlanStatus.InError, loadedAggregateFromDatabase.Status);
		}

		protected override Repository<ISeatPlan> TestRepository (IUnitOfWork unitOfWork)
		{
			return new SeatPlanRepository(unitOfWork); 
		}

		[Test]
		public void ShouldChangeSeatPlanStatus()
		{
			var seatPlan = new SeatPlan()
			{
				
				Date = new DateOnly(2015, 10, 2),
				Status = SeatPlanStatus.InProgress

			};

			PersistAndRemoveFromUnitOfWork(seatPlan);

			var seatPlanRepo = new SeatPlanRepository (UnitOfWork);
			var existingSeatPlan = seatPlanRepo.GetSeatPlanForDate (seatPlan.Date);
			existingSeatPlan.Status = SeatPlanStatus.Ok;
			seatPlanRepo.Update(existingSeatPlan);

			var loaded = seatPlanRepo.LoadAggregate(seatPlan.Id.Value);
			Assert.AreEqual(loaded.Status, SeatPlanStatus.Ok);
			Assert.AreEqual(loaded.Id, seatPlan.Id);
		}

		#region Utility Methods

		private ISeat createSeatMapLocationAndSeatInDb()
		{
			var rep = new Repository(UnitOfWork);
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{DummyData}", "TestLocation");
			seatMapLocation.AddSeat("Test Seat", 0);
			rep.Add(seatMapLocation);
			return seatMapLocation.Seats.First();
		}

		#endregion
	}
}