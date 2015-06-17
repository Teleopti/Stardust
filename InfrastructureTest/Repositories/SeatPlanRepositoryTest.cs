using System;
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
				Status = SeatPlanStatus.Ok
			};

			return seatPlan;
			
		}

		protected override void VerifyAggregateGraphProperties (ISeatPlan loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.Id);
			Assert.IsNotNull(loadedAggregateFromDatabase.Date);
			Assert.IsNotNull(loadedAggregateFromDatabase.Status);
		}

		protected override Repository<ISeatPlan> TestRepository (IUnitOfWork unitOfWork)
		{
			return new SeatPlanRepository(unitOfWork); 
			
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