using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.SeatPlanner.Controllers
{
	
	internal class SeatMapControllerTest
	{
		private ISeatMapLocationRepository _seatMapLocationRepository ;
		private SeatMapController _seatMapController;
		private SeatMapProvider _seatMapProvider;
		private ICommandDispatcher _commandDispatcher;
		private ILoggedOnUser _loggedOnUser;

		//Robtodo: Fix tests!

		[SetUp]
		public void Setup()
		{
			_seatMapLocationRepository = MockRepository.GenerateMock<ISeatMapLocationRepository>();
			_commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			_loggedOnUser = new FakeLoggedOnUser();
			_seatMapProvider = new SeatMapProvider(_seatMapLocationRepository);
			_seatMapController = new SeatMapController(_seatMapProvider, _commandDispatcher,_loggedOnUser );
		}

		[Test]
		public void ShouldGetSeatMapLocation()
		{
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{}", "Location1");
			seatMapLocation.SetId(Guid.NewGuid());

			_seatMapLocationRepository.Stub(x => x.LoadAggregate(seatMapLocation.Id.Value)).Return(seatMapLocation);

			var result = _seatMapController.Get(seatMapLocation.Id);

			Assert.True(result.GetType() == typeof(LocationViewModel));
			Assert.True(result.Id == seatMapLocation.Id);
			Assert.True(result.Name == seatMapLocation.Name);
		}
	}
}
